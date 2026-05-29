using Library.Crypto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WalletApp.Data.Models;

namespace WalletApp.Services
{
    /// <summary>
    /// Implements the Holder Coordinator role (§3.6, §3.7) by making real HTTP calls
    /// to the IssuerApp's VCALM endpoints.
    ///
    /// Full DIDAuthentication two-round-trip flow:
    ///   Round-trip 1: POST {} → receive VerifiablePresentationRequest with challenge.
    ///   Round-trip 2: Sign a DIDAuth VP with the holder's Ed25519 key and POST it back.
    ///                 Receive the signed AchievementCredential in a VerifiablePresentation.
    /// </summary>
    public class HolderExchangeService : IHolderExchangeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEd25519SigningService _signingService;
        private readonly IJsonLdCanonicalizationService _canonicalizationService;
        private readonly ILogger<HolderExchangeService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public HolderExchangeService(
            IHttpClientFactory httpClientFactory,
            IEd25519SigningService signingService,
            IJsonLdCanonicalizationService canonicalizationService,
            ILogger<HolderExchangeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _signingService = signingService;
            _canonicalizationService = canonicalizationService;
            _logger = logger;
        }

        public async Task<HolderExchangeResult> ReceiveCredentialAsync(
            string interactionUrl, Holder? holder = null)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient.Default);

            try
            {
                // ---------------------------------------------------------------
                // Step 1: GET Interaction URL → Protocols  (§3.7.1)
                // ---------------------------------------------------------------
                _logger.LogInformation("Step 1: GET {Url}", interactionUrl);

                var protocolsResponse = await client.GetAsync(interactionUrl);
                if (!protocolsResponse.IsSuccessStatusCode)
                    return Fail($"GET {interactionUrl} returned {(int)protocolsResponse.StatusCode}");

                var protocols = await protocolsResponse.Content
                    .ReadFromJsonAsync<ProtocolsDto>(_jsonOptions);

                if (string.IsNullOrWhiteSpace(protocols?.Vcapi))
                    return Fail("Protocols response did not contain a 'vcapi' exchange URL.");

                _logger.LogInformation(
                    "Step 1: Protocols received. vcapi={Vcapi} inviteRequest={InviteRequest}",
                    protocols.Vcapi, protocols.InviteRequest);

                // ---------------------------------------------------------------
                // Step 2: POST InviteResponse → invite-request/response  (§3.7)
                // ---------------------------------------------------------------
                if (!string.IsNullOrWhiteSpace(protocols.InviteRequest))
                {
                    _logger.LogInformation("Step 2: POST InviteResponse to {Url}", protocols.InviteRequest);

                    var inviteResult = await client.PostAsJsonAsync(
                        protocols.InviteRequest,
                        new InviteResponseDto
                        {
                            Purpose = "AchievementCredential issuance",
                            ReferenceId = $"urn:uuid:{Guid.NewGuid()}"
                        });

                    _logger.LogInformation(
                        "Step 2: InviteResponse status = {Status}", (int)inviteResult.StatusCode);
                }

                // ---------------------------------------------------------------
                // Step 3 (Round-trip 1): POST {} → receive DIDAuthentication VPR
                // ---------------------------------------------------------------
                _logger.LogInformation("Step 3: POST {{}} to exchange URL {Url}", protocols.Vcapi);

                var rt1Result = await client.PostAsJsonAsync(protocols.Vcapi, new { });
                if (!rt1Result.IsSuccessStatusCode)
                {
                    var err = await rt1Result.Content.ReadAsStringAsync();
                    return Fail($"Round-trip 1 returned {(int)rt1Result.StatusCode}: {err}");
                }

                var rt1Json = await rt1Result.Content.ReadAsStringAsync();
                _logger.LogInformation("Step 3: Round-trip 1 response received ({Bytes} bytes).", rt1Json.Length);

                // Parse the challenge and domain from the VPR.
                using var rt1Doc = JsonDocument.Parse(rt1Json);
                if (!rt1Doc.RootElement.TryGetProperty("verifiablePresentationRequest", out var vprEl))
                    return Fail("Round-trip 1 response did not contain a verifiablePresentationRequest.");

                if (!vprEl.TryGetProperty("challenge", out var challengeEl) ||
                    challengeEl.GetString() is not { Length: > 0 } challenge)
                    return Fail("VPR did not contain a challenge.");

                vprEl.TryGetProperty("domain", out var domainEl);
                var domain = domainEl.GetString() ?? string.Empty;

                _logger.LogInformation(
                    "Step 3: DIDAuthentication VPR received. challenge={Challenge} domain={Domain}",
                    challenge, domain);

                // ---------------------------------------------------------------
                // Step 4 (Round-trip 2): Build and sign a DIDAuth VP, POST it back.
                // ---------------------------------------------------------------
                if (holder is null || string.IsNullOrWhiteSpace(holder.SigningPrivateKeyBase64))
                {
                    return Fail(
                        "Cannot complete DIDAuthentication: no holder with a signing key is available. " +
                        "Ensure the user has a SelectedHolder with a generated key pair.");
                }

                var pubKeyMultibase = holder.SigningPublicKeyMultibase
                    ?? _signingService.PublicKeyMultibaseFromPrivate(holder.SigningPrivateKeyBase64);

                // Use the holder's did:web as the VM id — the fragment key-1 matches
                // what DidWebHolderService publishes in the DID document.
                var vmId = $"{holder.Id}#key-1";

                // Build the VP (without proof) to canonicalize.
                var vpWithoutProof = new Dictionary<string, object>
                {
                    ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                    ["type"] = new[] { "VerifiablePresentation" },
                    ["holder"] = holder.Id
                };

                // Build the proof options document.
                var created = DateTimeOffset.UtcNow;
                var proofOptions = new Dictionary<string, object>
                {
                    ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                    ["type"] = "DataIntegrityProof",
                    ["cryptosuite"] = "eddsa-rdfc-2022",
                    ["created"] = created.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["verificationMethod"] = vmId,
                    ["proofPurpose"] = "authentication",
                    ["challenge"] = challenge,
                    ["domain"] = domain
                };

                // Canonicalize both documents and hash.
                var proofOptionsBytes = _canonicalizationService.Canonicalize(
                    JsonSerializer.Serialize(proofOptions));
                var vpBytes = _canonicalizationService.Canonicalize(
                    JsonSerializer.Serialize(vpWithoutProof));

                var proofInput = new byte[32 + 32];
                SHA256.HashData(proofOptionsBytes).CopyTo(proofInput, 0);
                SHA256.HashData(vpBytes).CopyTo(proofInput, 32);

                var proofValue = _signingService.Sign(holder.SigningPrivateKeyBase64, proofInput);

                // Attach the proof to the VP.
                var vpWithProof = new Dictionary<string, object>(vpWithoutProof)
                {
                    ["proof"] = new Dictionary<string, object>
                    {
                        ["type"] = "DataIntegrityProof",
                        ["cryptosuite"] = "eddsa-rdfc-2022",
                        ["created"] = created.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        ["verificationMethod"] = vmId,
                        ["proofPurpose"] = "authentication",
                        ["challenge"] = challenge,
                        ["domain"] = domain,
                        ["proofValue"] = proofValue
                    }
                };

                _logger.LogInformation(
                    "Step 4: Posting DIDAuth VP (holder={HolderDid}, vm={Vm})", holder.Id, vmId);

                var rt2Body = new Dictionary<string, object> { ["verifiablePresentation"] = vpWithProof };
                var rt2Result = await client.PostAsJsonAsync(protocols.Vcapi, rt2Body);

                if (!rt2Result.IsSuccessStatusCode)
                {
                    var err = await rt2Result.Content.ReadAsStringAsync();
                    return Fail($"Round-trip 2 returned {(int)rt2Result.StatusCode}: {err}");
                }

                var serverMessageJson = await rt2Result.Content.ReadAsStringAsync();
                _logger.LogInformation(
                    "Step 4: Exchange complete. Server message received ({Bytes} bytes).",
                    serverMessageJson.Length);

                // ---------------------------------------------------------------
                // Step 5: Extract the VerifiablePresentation from the server message.
                // ---------------------------------------------------------------
                string? vpJson = null;
                try
                {
                    using var doc = JsonDocument.Parse(serverMessageJson);
                    if (doc.RootElement.TryGetProperty("verifiablePresentation", out var vpElement))
                    {
                        vpJson = JsonSerializer.Serialize(vpElement, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Could not parse VP from server message.");
                }

                return new HolderExchangeResult
                {
                    Success = true,
                    ServerMessageJson = serverMessageJson,
                    VerifiablePresentationJson = vpJson
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during exchange for URL {Url}", interactionUrl);
                return Fail($"Unexpected error: {ex.Message}");
            }
        }

        private static HolderExchangeResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };

        private sealed class ProtocolsDto
        {
            [JsonPropertyName("vcapi")]
            public string? Vcapi { get; set; }

            [JsonPropertyName("inviteRequest")]
            public string? InviteRequest { get; set; }
        }

        private sealed class InviteResponseDto
        {
            [JsonPropertyName("purpose")]
            public string? Purpose { get; set; }

            [JsonPropertyName("referenceId")]
            public string? ReferenceId { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }
        }
    }
}
