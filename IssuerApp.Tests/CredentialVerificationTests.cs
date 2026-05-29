using IssuerApp.Data;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Services;
using Library.Crypto;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IssuerApp.Integration.Tests
{
    /// <summary>
    /// Integration tests that issue an AchievementCredential via the VC-API exchange
    /// endpoint and then POST it to the 1EdTech public validator for independent
    /// verification.
    ///
    /// Running the validator locally:
    ///   Prerequisites: Java 17+, Maven 3.6+
    ///   1. Clone https://github.com/1EdTech/digital-credentials-public-validator
    ///   2. cd inspector-vc-web
    ///   3. mvn spring-boot:run
    ///   4. Validator listens on http://localhost:8080
    ///      Swagger UI: http://localhost:8080/swagger-ui.html
    ///
    /// Tests that require the validator are skipped automatically when it is not
    /// reachable, so they do not fail in CI.
    /// </summary>
    public class CredentialVerificationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private const string ValidatorBaseUrl = "http://localhost:8080";
        private const string ValidatorVerifyEndpoint = "/api/validate";

        private readonly CustomWebApplicationFactory _factory;
        private static readonly HttpClient _validatorClient = new()
        {
            BaseAddress = new Uri(ValidatorBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        public CredentialVerificationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Seeds a minimal but complete AchievementCredential (with signing key)
        /// and creates an exchange for it. Returns the exchange ID and a holder key pair
        /// that can be used for the DIDAuthentication round-trip.
        /// </summary>
        private static async Task<(int credentialKey, string exchangeId, string holderPublicKeyMultibase, string holderPrivateKeyBase64)> SeedCredentialAndExchangeAsync(
            IServiceScope scope)
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var exchangeService = scope.ServiceProvider.GetRequiredService<IExchangeService>();
            var signingService = scope.ServiceProvider.GetRequiredService<IEd25519SigningService>();

            // Generate a fresh key pair for this test credential.
            var (publicKeyMultibase, privateKeyBase64) = signingService.GenerateKeyPair();

            // Reuse the organization already seeded by CustomWebApplicationFactory (key=1)
            // and add signing keys to it.
            var org = await db.Organizations.FindAsync(1);
            if (org is null)
            {
                throw new InvalidOperationException("Organization with key 1 not found in test database.");
            }

            org.SigningPublicKeyMultibase = publicKeyMultibase;
            org.SigningPrivateKeyBase64 = privateKeyBase64;
            await db.SaveChangesAsync();

            // Build a minimal AchievementCredential.
            var member = await db.Members.FindAsync(1);
            if (member is null)
            {
                throw new InvalidOperationException("Member with key 1 not found in test database.");
            }

            var achievement = new Achievement
            {
                Id = "https://example.com/achievements/test-badge",
                Type = new[] { "Achievement" },
                Name = "Test Badge",
                Description = "Awarded for testing.",
                Criteria = new Criteria
                {
                    Narrative = "Complete the test."
                },
                OrganizationKey = org.OrganizationKey
            };
            db.Achievements.Add(achievement);
            _ = achievement.AdditionalProperties;
            _ = achievement.Criteria!.AdditionalProperties;
            await db.SaveChangesAsync();

            var subject = new AchievementSubject
            {
                Type = new[] { "AchievementSubject" },
                Achievement = achievement,
                MemberKey = member.MemberKey
            };
            db.AchievementSubjects.Add(subject);
            _ = subject.AdditionalProperties;
            await db.SaveChangesAsync();

            var issuerProfile = await db.Profiles.FindAsync(1);
            if (issuerProfile is null)
            {
                throw new InvalidOperationException("Profile with key 1 not found in test database.");
            }

            var credential = new AchievementCredential
            {
                Id = $"https://example.com/credentials/{Guid.NewGuid()}",
                Issuer = issuerProfile,
                ValidFrom = DateTimeOffset.UtcNow,
                CredentialSubject = subject,
                OrganizationKey = org.OrganizationKey
            };
            db.AchievementCredentials.Add(credential);
            _ = credential.AdditionalProperties;
            await db.SaveChangesAsync();

            // Also generate a holder key pair for DIDAuthentication.
            var (holderPublicKeyMultibase, holderPrivateKeyBase64) = signingService.GenerateKeyPair();

            var exchange = exchangeService.CreateExchange(credential.AchievementCredentialKey);
            return (credential.AchievementCredentialKey, exchange.ExchangeId, holderPublicKeyMultibase, holderPrivateKeyBase64);
        }

        /// <summary>
        /// Returns true when the 1EdTech validator is reachable at localhost:8080.
        /// </summary>
        private static async Task<bool> IsValidatorRunningAsync()
        {
            try
            {
                var resp = await _validatorClient.GetAsync("/actuator/health");
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ------------------------------------------------------------------
        // Tests
        // ------------------------------------------------------------------

        /// <summary>
        /// Issues a credential via the VC-API exchange endpoint and checks that
        /// the response contains a signed DataIntegrityProof with cryptosuite
        /// <c>eddsa-rdfc-2022</c>.
        /// </summary>
        [Fact]
        public async Task IssuedCredential_HasEddsaRdfc2022Proof()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var (_, exchangeId, holderPublicKeyMultibase, holderPrivateKeyBase64) =
                await SeedCredentialAndExchangeAsync(scope);

            var signingService = scope.ServiceProvider.GetRequiredService<IEd25519SigningService>();
            var canonicalizationService = scope.ServiceProvider.GetRequiredService<IJsonLdCanonicalizationService>();

            var workflowId = "default";
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act — Round-trip 1: POST empty body to get DIDAuthentication VPR.
            var rt1Response = await client.PostAsync(
                $"/workflows/{workflowId}/exchanges/{exchangeId}",
                new StringContent("{}", Encoding.UTF8, "application/json"),
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, rt1Response.StatusCode);
            var rt1Json = await rt1Response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var rt1Doc = JsonDocument.Parse(rt1Json);
            var vpr = rt1Doc.RootElement.GetProperty("verifiablePresentationRequest");
            var challenge = vpr.GetProperty("challenge").GetString()!;
            var domain = vpr.GetProperty("domain").GetString()!;

            // Build a DIDAuthentication VP signed with the holder's key.
            var holderDid = $"did:key:{holderPublicKeyMultibase}";
            var vmId = $"{holderDid}#{holderPublicKeyMultibase}";

            var vpWithoutProof = new Dictionary<string, object>
            {
                ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                ["type"] = new[] { "VerifiablePresentation" },
                ["holder"] = holderDid
            };

            var proofOptions = new Dictionary<string, object>
            {
                ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                ["type"] = "DataIntegrityProof",
                ["cryptosuite"] = "eddsa-rdfc-2022",
                ["created"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["verificationMethod"] = vmId,
                ["proofPurpose"] = "authentication",
                ["challenge"] = challenge,
                ["domain"] = domain
            };

            var vpBytes = canonicalizationService.Canonicalize(JsonSerializer.Serialize(vpWithoutProof));
            var proofOptionsBytes = canonicalizationService.Canonicalize(JsonSerializer.Serialize(proofOptions));
            var proofInput = new byte[32 + 32];
            SHA256.HashData(proofOptionsBytes).CopyTo(proofInput, 0);
            SHA256.HashData(vpBytes).CopyTo(proofInput, 32);
            var proofValue = signingService.Sign(holderPrivateKeyBase64, proofInput);

            var didAuthProof = new Dictionary<string, object>(proofOptions) { ["proofValue"] = proofValue };
            didAuthProof.Remove("@context");
            var vpWithProof = new Dictionary<string, object>(vpWithoutProof) { ["proof"] = didAuthProof };

            // Act — Round-trip 2: POST DIDAuth VP.
            var rt2Body = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["verifiablePresentation"] = vpWithProof
            });
            var response = await client.PostAsync(
                $"/workflows/{workflowId}/exchanges/{exchangeId}",
                new StringContent(rt2Body, Encoding.UTF8, "application/json"),
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);

            // Navigate to verifiableCredential[0].proof[0].cryptosuite
            var vp = doc.RootElement
                .GetProperty("verifiablePresentation");
            var vc = vp.GetProperty("verifiableCredential")[0];
            var proof = vc.GetProperty("proof");

            // proof may be an object or array
            JsonElement proofObj = proof.ValueKind == JsonValueKind.Array
                ? proof[0]
                : proof;

            Assert.Equal("DataIntegrityProof", proofObj.GetProperty("type").GetString());
            Assert.Equal("eddsa-rdfc-2022", proofObj.GetProperty("cryptosuite").GetString());
            Assert.Equal("assertionMethod", proofObj.GetProperty("proofPurpose").GetString());

            var credProofValue = proofObj.GetProperty("proofValue").GetString();
            Assert.NotNull(credProofValue);
            Assert.StartsWith("z", credProofValue); // multibase base58btc
        }

        /// <summary>
        /// Issues a credential and submits it to the local 1EdTech public validator.
        /// The test is skipped automatically when the validator is not running.
        ///
        /// To run this test:
        ///   1. Start the validator: cd inspector-vc-web &amp;&amp; mvn spring-boot:run
        ///   2. Run the test — it will POST the issued VC to http://localhost:8080/api/validate.
        /// </summary>
        [Fact]
        public async Task IssuedCredential_PassesOnEdTechValidator()
        {
            if (!await IsValidatorRunningAsync())
            {
                // Skip gracefully when the validator is not running locally.
                Assert.Skip("Validator not running.");
            }

            // Arrange
            using var scope = _factory.Services.CreateScope();
            var (_, exchangeId, holderPublicKeyMultibase, holderPrivateKeyBase64) =
                await SeedCredentialAndExchangeAsync(scope);

            var signingService = scope.ServiceProvider.GetRequiredService<IEd25519SigningService>();
            var canonicalizationService = scope.ServiceProvider.GetRequiredService<IJsonLdCanonicalizationService>();

            var workflowId = "default";
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Round-trip 1: get DIDAuthentication VPR.
            var rt1Response = await client.PostAsync(
                $"/workflows/{workflowId}/exchanges/{exchangeId}",
                new StringContent("{}", Encoding.UTF8, "application/json"),
                TestContext.Current.CancellationToken);
            rt1Response.EnsureSuccessStatusCode();
            var rt1Json = await rt1Response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var rt1Doc = JsonDocument.Parse(rt1Json);
            var vprEl = rt1Doc.RootElement.GetProperty("verifiablePresentationRequest");
            var challenge = vprEl.GetProperty("challenge").GetString()!;
            var domain = vprEl.GetProperty("domain").GetString()!;

            var holderDid = $"did:key:{holderPublicKeyMultibase}";
            var vmId = $"{holderDid}#{holderPublicKeyMultibase}";
            var vpNoProof = new Dictionary<string, object>
            {
                ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                ["type"] = new[] { "VerifiablePresentation" },
                ["holder"] = holderDid
            };
            var poDict = new Dictionary<string, object>
            {
                ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                ["type"] = "DataIntegrityProof",
                ["cryptosuite"] = "eddsa-rdfc-2022",
                ["created"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["verificationMethod"] = vmId,
                ["proofPurpose"] = "authentication",
                ["challenge"] = challenge,
                ["domain"] = domain
            };
            var vpB = canonicalizationService.Canonicalize(JsonSerializer.Serialize(vpNoProof));
            var poB = canonicalizationService.Canonicalize(JsonSerializer.Serialize(poDict));
            var pInput = new byte[32 + 32];
            SHA256.HashData(poB).CopyTo(pInput, 0);
            SHA256.HashData(vpB).CopyTo(pInput, 32);
            var pValue = signingService.Sign(holderPrivateKeyBase64, pInput);
            var proofDict = new Dictionary<string, object>(poDict) { ["proofValue"] = pValue };
            proofDict.Remove("@context");
            var vpFull = new Dictionary<string, object>(vpNoProof) { ["proof"] = proofDict };

            // Round-trip 2: send DIDAuth VP and receive credential.
            var issueResponse = await client.PostAsync(
                $"/workflows/{workflowId}/exchanges/{exchangeId}",
                new StringContent(JsonSerializer.Serialize(new Dictionary<string, object>
                { ["verifiablePresentation"] = vpFull }),
                    Encoding.UTF8, "application/json"),
                TestContext.Current.CancellationToken);

            issueResponse.EnsureSuccessStatusCode();
            var vpJson = await issueResponse.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            // Extract the verifiablePresentation from the server message wrapper.
            using var serverMsg = JsonDocument.Parse(vpJson);
            var vpElement = serverMsg.RootElement.GetProperty("verifiablePresentation");
            var vpJsonString = vpElement.GetRawText();

            // POST the VC to the 1EdTech validator as multipart/form-data.
            // Endpoint: POST /api/validate?validatorId=OB30Inspector
            // Body: field "file" containing the VC JSON bytes.
            var vcElement = vpElement.GetProperty("verifiableCredential")[0];
            var vcJson = vcElement.GetRawText();
            var fileBytes = Encoding.UTF8.GetBytes(vcJson);

            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            form.Add(fileContent, "file", "credential.json");

            var verifyResponse = await _validatorClient.PostAsync(
                $"{ValidatorVerifyEndpoint}?validatorId=OB30Inspector",
                form,
                TestContext.Current.CancellationToken);

            var verifyResult = await verifyResponse.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.True(
                verifyResponse.IsSuccessStatusCode,
                $"Validator returned {(int)verifyResponse.StatusCode}: {verifyResult}");

            // The Report.summary.outcome should be VALID (no errors).
            using var resultDoc = JsonDocument.Parse(verifyResult);
            var summary = resultDoc.RootElement.GetProperty("summary");
            var outcome = summary.GetProperty("outcome").GetString();
            var errorCount = summary.GetProperty("errors").GetInt32();
            var fatalCount = summary.GetProperty("fatals").GetInt32();
            Assert.True(
                outcome == "VALID" || outcome == "WARNING",
                $"Validator outcome was '{outcome}' with {errorCount} error(s), {fatalCount} fatal(s).\n{verifyResult}");
            Assert.Equal(0, errorCount);
            Assert.Equal(0, fatalCount);
        }
    }
}
