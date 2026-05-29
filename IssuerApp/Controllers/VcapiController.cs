using IssuerApp.Data;
using IssuerApp.Services;
using Library.Models.OpenBadges.Converters;
using Library.Models.Vc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IssuerApp.Controllers
{
    /// <summary>
    /// Implements the VCALM (VC API for Lifecycle Management) server-side endpoints
    /// required of an Issuer/Coordinator.
    ///
    /// Spec sections implemented:
    ///   §3.7.1  GET  /interactions/{interactionId}?iuv=1
    ///           Returns the Protocols object so the wallet can discover the exchange URL.
    ///
    ///   §3.7    POST /{inviteId}/invite-request/response
    ///           Receives the wallet's InviteResponse (opt-in + optional callback URL).
    ///
    ///   §3.6    GET  /workflows/{workflowId}/exchanges/{exchangeId}
    ///           Returns the current state of an exchange.
    ///
    ///   §3.6    POST /workflows/{workflowId}/exchanges/{exchangeId}  (round-trip 1)
    ///           Wallet sends empty body; issuer responds with a DIDAuthentication VPR
    ///           containing a one-time challenge.
    ///
    ///   §3.6    POST /workflows/{workflowId}/exchanges/{exchangeId}  (round-trip 2)
    ///           Wallet sends a VP signed with the holder's DID key; issuer verifies the
    ///           proof, extracts the holder DID, signs the credential and returns it.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("")]
    public class VcapiController : ControllerBase
    {
        private readonly IExchangeService _exchangeService;
        private readonly IStatusListService _statusListService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VcapiController> _logger;
        private readonly IEd25519SigningService _signingService;
        private readonly IJsonLdCanonicalizationService _canonicalizationService;
        private readonly IHttpClientFactory _httpClientFactory;

        // Options used when serializing the AchievementCredential into the VP payload.
        // The Library models carry [JsonPropertyName] attributes (e.g. "@context") so
        // camelCase policy is only needed for the VP/server-message wrapper dictionaries.
        private static readonly JsonSerializerOptions _credentialSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters = { new AchievementCredentialConverter() }
        };

        public VcapiController(
            IExchangeService exchangeService,
            IStatusListService statusListService,
            ApplicationDbContext context,
            ILogger<VcapiController> logger,
            IEd25519SigningService signingService,
            IJsonLdCanonicalizationService canonicalizationService,
            IHttpClientFactory httpClientFactory)
        {
            _exchangeService = exchangeService;
            _statusListService = statusListService;
            _context = context;
            _logger = logger;
            _signingService = signingService;
            _canonicalizationService = canonicalizationService;
            _httpClientFactory = httpClientFactory;
        }

        // -----------------------------------------------------------------
        // §3.7.1  GET /interactions/{interactionId}?iuv=1
        // -----------------------------------------------------------------

        /// <summary>
        /// The wallet GETs this URL to discover which protocols are available.
        /// Returns a Protocols object with the VC-API exchange URL and the
        /// invite-request/response URL.
        /// </summary>
        [HttpGet("interactions/{interactionId}")]
        public IActionResult GetInteraction(string interactionId, [FromQuery] string? iuv)
        {
            var exchange = _exchangeService.GetExchange(interactionId);
            if (exchange is null)
                return NotFound();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var protocols = new ProtocolsResponse
            {
                Vcapi = $"{baseUrl}/workflows/{exchange.WorkflowId}/exchanges/{exchange.ExchangeId}",
                InviteRequest = $"{baseUrl}/{exchange.ExchangeId}/invite-request/response"
            };

            _logger.LogInformation(
                "Interaction {ExchangeId}: Protocols returned (iuv={Iuv})",
                interactionId, iuv);

            return Ok(protocols);
        }

        // -----------------------------------------------------------------
        // §3.7  POST /{inviteId}/invite-request/response
        // -----------------------------------------------------------------

        /// <summary>
        /// The wallet POSTs an InviteResponse here to opt in to the interaction.
        /// The response body is ingested and stored; the wallet then proceeds to
        /// POST to the exchange URL (Protocols.vcapi).
        /// </summary>
        [HttpPost("{inviteId}/invite-request/response")]
        public IActionResult ReceiveInviteResponse(
            string inviteId,
            [FromBody] InviteResponseRequest? body)
        {
            var exchange = _exchangeService.GetExchange(inviteId);
            if (exchange is null)
                return NotFound();

            _exchangeService.RecordInviteResponse(
                inviteId,
                body?.ReferenceId,
                body?.Url);

            _logger.LogInformation(
                "Exchange {ExchangeId}: InviteResponse received (purpose='{Purpose}')",
                inviteId, body?.Purpose);

            return Ok();
        }

        // -----------------------------------------------------------------
        // §3.6  GET /workflows/{workflowId}/exchanges/{exchangeId}
        // -----------------------------------------------------------------

        /// <summary>Returns the current state of an exchange.</summary>
        [HttpGet("workflows/{workflowId}/exchanges/{exchangeId}")]
        public IActionResult GetExchangeState(string workflowId, string exchangeId)
        {
            var exchange = _exchangeService.GetExchange(exchangeId);
            if (exchange is null)
                return NotFound();

            return Ok(new ExchangeStateResponse
            {
                Id = exchange.ExchangeId,
                State = exchange.State,
                Sequence = exchange.Sequence,
                Step = "issue",
                Expires = exchange.Expires.ToString("O")
            });
        }

        // -----------------------------------------------------------------
        // §3.6  POST /workflows/{workflowId}/exchanges/{exchangeId}
        // -----------------------------------------------------------------

        /// <summary>
        /// Exchange participation endpoint (Holder Coordinator → Issuer Coordinator).
        ///
        /// DIDAuthentication issuance workflow (two round-trips):
        ///
        ///   Round-trip 1 — wallet POSTs empty body {}:
        ///     Issuer generates a one-time challenge, stores it on the exchange, and
        ///     responds with an ExchangeParticipationServerMessage containing a
        ///     VerifiablePresentationRequest with query type "DIDAuthentication".
        ///
        ///   Round-trip 2 — wallet POSTs verifiablePresentation:
        ///     Issuer resolves the holder's DID document to obtain the public key,
        ///     verifies the eddsa-rdfc-2022 proof over the challenge, extracts the
        ///     holder DID, signs the credential and returns the credential wrapped in a VerifiablePresentation.
        /// </summary>
        [HttpPost("workflows/{workflowId}/exchanges/{exchangeId}")]
        public async Task<IActionResult> ParticipateInExchange(
            string workflowId,
            string exchangeId,
            [FromBody] JsonElement? body)
        {
            var exchange = _exchangeService.GetExchange(exchangeId);
            if (exchange is null)
                return NotFound(new ProblemDetailsResponse("Exchange not found", 404));

            if (exchange.State == "complete")
                return BadRequest(new ProblemDetailsResponse("Exchange already complete", 400));

            // -----------------------------------------------------------------
            // Round-trip 1: wallet sends {} (no verifiablePresentation in body)
            // → issuer returns a DIDAuthentication VPR with a fresh challenge.
            // -----------------------------------------------------------------
            if (exchange.Step is null)
            {
                var challenge = Guid.NewGuid().ToString();
                var domain = Request.Host.Value ?? "localhost";
                _exchangeService.StoreChallengeForDIDAuth(exchangeId, challenge);

                var vpr = new Dictionary<string, object>
                {
                    ["query"] = new[] { new Dictionary<string, string> { ["type"] = "DIDAuthentication" } },
                    ["challenge"] = challenge,
                    ["domain"] = domain
                };

                var serverMessage = new Dictionary<string, object>
                {
                    ["referenceId"] = $"urn:uuid:{Guid.NewGuid()}",
                    ["verifiablePresentationRequest"] = vpr
                };

                _logger.LogInformation(
                    "Exchange {ExchangeId}: Round-trip 1 — DIDAuthentication VPR sent (challenge={Challenge})",
                    exchangeId, challenge);

                return Ok(serverMessage);
            }

            // -----------------------------------------------------------------
            // Round-trip 2: wallet sends { verifiablePresentation: { … } }
            // → issuer verifies the DIDAuth VP proof, then issues the credential.
            // -----------------------------------------------------------------
            if (exchange.Step == "AwaitingDIDAuth")
            {
                if (!body.HasValue ||
                    !body.Value.TryGetProperty("verifiablePresentation", out var vpElement))
                {
                    return BadRequest(new ProblemDetailsResponse(
                        "Expected verifiablePresentation in body for DIDAuthentication step.", 400));
                }

                // Extract the holder DID from the VP's proof.verificationMethod
                // (format: "<holderDid>#<keyFragment>").
                if (!vpElement.TryGetProperty("proof", out var proofEl) ||
                    !proofEl.TryGetProperty("verificationMethod", out var vmEl) ||
                    vmEl.GetString() is not { Length: > 0 } verificationMethod)
                {
                    return BadRequest(new ProblemDetailsResponse(
                        "VP proof.verificationMethod is missing or empty.", 400));
                }

                var holderDid = verificationMethod.Contains('#')
                    ? verificationMethod[..verificationMethod.IndexOf('#')]
                    : verificationMethod;

                // Resolve the holder's DID document to obtain the public key.
                var publicKeyMultibase = await ResolvePublicKeyAsync(holderDid, verificationMethod);
                if (publicKeyMultibase is null)
                {
                    return BadRequest(new ProblemDetailsResponse(
                        $"Could not resolve public key for verificationMethod '{verificationMethod}'.", 400));
                }

                // Verify the eddsa-rdfc-2022 proof on the VP.
                var vpJson = vpElement.GetRawText();
                var verifyResult = VerifyDIDAuthProof(vpJson, exchange.Challenge!, publicKeyMultibase);
                if (!verifyResult.Success)
                {
                    _logger.LogWarning(
                        "Exchange {ExchangeId}: DIDAuth proof verification failed — {Reason}",
                        exchangeId, verifyResult.FailureReason);
                    return BadRequest(new ProblemDetailsResponse(
                        $"DIDAuthentication proof verification failed: {verifyResult.FailureReason}", 400));
                }

                _exchangeService.StoreHolderDid(exchangeId, holderDid);

                _logger.LogInformation(
                    "Exchange {ExchangeId}: Round-trip 2 — DIDAuth proof verified. holderDid={HolderDid}",
                    exchangeId, holderDid);

                // Fall through to credential issuance below.
            }
            else
            {
                return BadRequest(new ProblemDetailsResponse(
                    $"Unexpected exchange step '{exchange.Step}'.", 400));
            }

            // -----------------------------------------------------------------
            // Issue the credential — runs after a successful DIDAuth verification.
            // -----------------------------------------------------------------

            // Load the credential with all relationships required for JSON-LD serialization.
            var credential = await _context.AchievementCredentials
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Achievement)
                    .ThenInclude(a => a!.Criteria)
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Member)
                .Include(ac => ac.CredentialSubject)
                    .ThenInclude(cs => cs!.Source)
                .Include(ac => ac.Image)
                .Include(ac => ac.Organization)
                    .ThenInclude(o => o!.Profile)
                .FirstOrDefaultAsync(ac =>
                    ac.AchievementCredentialKey == exchange.AchievementCredentialKey);

            if (credential is null)
                return NotFound(new ProblemDetailsResponse("Credential not found", 404));

            // Populate Issuer from the Organization profile if not already set.
            if (credential.Issuer is null && credential.Organization?.Profile is not null)
                credential.Issuer = credential.Organization.Profile;

            // Bind the proven holder DID as credentialSubject.id.
            if (!string.IsNullOrWhiteSpace(exchange.HolderDid) && credential.CredentialSubject is not null)
            {
                credential.CredentialSubject.Id = exchange.HolderDid;
                _logger.LogInformation(
                    "Exchange {ExchangeId}: CredentialSubject.Id set to proven holder DID {HolderDid}",
                    exchangeId, exchange.HolderDid);
            }

            // ----------------------------------------------------------------
            // Allocate (or reuse) a Bitstring Status List entry so the verifier
            // can check whether this credential has been revoked (VC 2.0 §5.9).
            // ----------------------------------------------------------------
            if (credential.StatusListIndex is null)
            {
                credential.StatusListIndex = _statusListService.AllocateIndex();
                _context.AchievementCredentials.Update(credential);
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Exchange {ExchangeId}: allocated status list index {Index} for AchievementCredentialKey={Key}",
                    exchangeId, credential.StatusListIndex, credential.AchievementCredentialKey);
            }

            // Embed credentialStatus so the issued JWT/JSON-LD carries the pointer.
            credential.CredentialStatus = new System.Collections.ObjectModel.Collection<Library.Models.Vc.CredentialStatus>
            {
                new Library.Models.Vc.CredentialStatus
                {
                    Id   = $"{_statusListService.StatusListEntryBaseUrl}-{credential.StatusListIndex}",
                    Type = "BitstringStatusListEntry",
                    AdditionalProperties = new Dictionary<string, object>
                    {
                        ["statusPurpose"]        = "revocation",
                        ["statusListIndex"]      = credential.StatusListIndex!.Value.ToString(),
                        ["statusListCredential"] = _statusListService.StatusListCredentialUrl
                    }
                }
            };

            // ----------------------------------------------------------------
            // eddsa-rdfc-2022 signing of the AchievementCredential
            // ----------------------------------------------------------------
            var signingOrg = credential.Organization;
            if (signingOrg?.SigningPrivateKeyBase64 is not null && credential.Issuer is not null)
            {
                var pubKeyMultibase = signingOrg.SigningPublicKeyMultibase
                    ?? _signingService.PublicKeyMultibaseFromPrivate(signingOrg.SigningPrivateKeyBase64);
                var didKey = $"did:key:{pubKeyMultibase}";
                var vmId = $"{didKey}#{pubKeyMultibase}";

                credential.Issuer.Id = didKey;
                credential.Proof = null;

                var credentialJson = JsonSerializer.Serialize(credential, _credentialSerializerOptions);
                var credentialBytes = _canonicalizationService.Canonicalize(credentialJson);
                var created = DateTimeOffset.UtcNow;

                var proofOptions = new Dictionary<string, object>
                {
                    ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                    ["type"] = "DataIntegrityProof",
                    ["cryptosuite"] = "eddsa-rdfc-2022",
                    ["created"] = created.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["verificationMethod"] = vmId,
                    ["proofPurpose"] = "assertionMethod"
                };
                var proofOptionsBytes = _canonicalizationService.Canonicalize(JsonSerializer.Serialize(proofOptions));

                var proofInput = new byte[32 + 32];
                SHA256.HashData(proofOptionsBytes).CopyTo(proofInput, 0);
                SHA256.HashData(credentialBytes).CopyTo(proofInput, 32);

                var proofValue = _signingService.Sign(signingOrg.SigningPrivateKeyBase64, proofInput);

                credential.Proof = new Collection<DataIntegrityProof>
                {
                    new DataIntegrityProof
                    {
                        Type = "DataIntegrityProof",
                        Cryptosuite = "eddsa-rdfc-2022",
                        Created = created,
                        VerificationMethod = vmId,
                        ProofPurpose = "assertionMethod",
                        ProofValue = proofValue
                    }
                };

                _logger.LogInformation(
                    "Exchange {ExchangeId}: credential signed with eddsa-rdfc-2022 (vm={Vm})",
                    exchangeId, vmId);
            }
            else
            {
                _logger.LogWarning(
                    "Exchange {ExchangeId}: organization has no signing key – credential issued unsigned",
                    exchangeId);
            }

            var credentialElement = JsonSerializer.SerializeToElement(credential, _credentialSerializerOptions);

            var vpDict = new Dictionary<string, object>
            {
                ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                ["type"] = new[] { "VerifiablePresentation" },
                ["verifiableCredential"] = new[] { credentialElement }
            };

            var finalServerMessage = new Dictionary<string, object>
            {
                ["referenceId"] = $"urn:uuid:{Guid.NewGuid()}",
                ["verifiablePresentation"] = JsonSerializer.SerializeToElement(vpDict)
            };

            _exchangeService.CompleteExchange(exchangeId);

            _logger.LogInformation(
                "Exchange {ExchangeId}: credential issued for AchievementCredentialKey={Key}",
                exchangeId, exchange.AchievementCredentialKey);

            return Ok(finalServerMessage);
        }

        // -----------------------------------------------------------------
        // DIDAuth VP proof verification
        // -----------------------------------------------------------------

        private record ProofVerifyResult(bool Success, string? FailureReason = null);

        /// <summary>
        /// Verifies the eddsa-rdfc-2022 proof on a DIDAuthentication VP.
        /// The VP must contain a proof with the expected challenge value.
        /// </summary>
        private ProofVerifyResult VerifyDIDAuthProof(
            string vpJson, string expectedChallenge, string publicKeyMultibase)
        {
            try
            {
                using var vpDoc = JsonDocument.Parse(vpJson);
                var root = vpDoc.RootElement;

                if (!root.TryGetProperty("proof", out var proofEl))
                    return new ProofVerifyResult(false, "VP has no proof.");

                // Verify the challenge matches what the issuer issued.
                if (!proofEl.TryGetProperty("challenge", out var challengeEl) ||
                    challengeEl.GetString() != expectedChallenge)
                    return new ProofVerifyResult(false, "VP proof.challenge does not match issued challenge.");

                if (!proofEl.TryGetProperty("proofValue", out var pvEl) ||
                    pvEl.GetString() is not { Length: > 0 } proofValue)
                    return new ProofVerifyResult(false, "VP proof.proofValue is missing.");

                // Reconstruct the signed bytes: SHA-256(proofOptions) || SHA-256(vpWithoutProof)
                // using the same eddsa-rdfc-2022 algorithm the wallet used to sign.
                var proofOptions = new Dictionary<string, object?>();
                foreach (var prop in proofEl.EnumerateObject())
                {
                    if (prop.Name == "proofValue") continue;
                    proofOptions[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => (object?)prop.Value.GetString(),
                        JsonValueKind.Array  => (object?)prop.Value.EnumerateArray()
                                .Select(e => e.GetString())
                                .ToArray(),
                        _ => (object?)prop.Value.GetRawText()
                    };
                }
                proofOptions["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" };

                // Build VP without proof for canonicalization.
                var vpWithoutProof = new Dictionary<string, object?>();
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name == "proof") continue;
                    vpWithoutProof[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
                }

                var proofOptionsBytes = _canonicalizationService.Canonicalize(
                    JsonSerializer.Serialize(proofOptions));
                var vpBytes = _canonicalizationService.Canonicalize(
                    JsonSerializer.Serialize(vpWithoutProof));

                var proofInput = new byte[32 + 32];
                SHA256.HashData(proofOptionsBytes).CopyTo(proofInput, 0);
                SHA256.HashData(vpBytes).CopyTo(proofInput, 32);

                if (!_signingService.Verify(publicKeyMultibase, proofInput, proofValue))
                    return new ProofVerifyResult(false, "Signature verification failed.");

                return new ProofVerifyResult(true);
            }
            catch (Exception ex)
            {
                return new ProofVerifyResult(false, ex.Message);
            }
        }

        // -----------------------------------------------------------------
        // DID document resolution
        // -----------------------------------------------------------------

        /// <summary>
        /// Resolves the public key multibase for a given verificationMethod ID by
        /// fetching the holder's DID document.
        ///
        /// Supports did:web (fetched via HTTP) and did:key (decoded inline).
        /// </summary>
        private async Task<string?> ResolvePublicKeyAsync(string holderDid, string verificationMethod)
        {
            if (holderDid.StartsWith("did:key:"))
            {
                // For did:key the public key IS the DID's method-specific identifier.
                // Format: did:key:<publicKeyMultibase>
                return holderDid["did:key:".Length..];
            }

            if (holderDid.StartsWith("did:web:"))
            {
                try
                {
                    // Convert did:web to an HTTPS URL per the did:web spec.
                    // did:web:host%3Aport:path:segments → https://host:port/path/segments/did.json
                    var methodSpecific = holderDid["did:web:".Length..];
                    var parts = methodSpecific.Split(':');
                    var host = Uri.UnescapeDataString(parts[0]);
                    var path = parts.Length > 1
                        ? string.Join("/", parts[1..]) + "/did.json"
                        : ".well-known/did.json";
                    var didDocUrl = $"https://{host}/{path}";

                    var client = _httpClientFactory.CreateClient(Constants.HttpClient.Default);
                    var didDoc = await client.GetFromJsonAsync<DidDocumentDto>(didDocUrl);

                    var vm = didDoc?.VerificationMethod?
                        .FirstOrDefault(v => v.Id == verificationMethod);

                    return vm?.PublicKeyMultibase;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Exchange: failed to resolve DID document for {Did}", holderDid);
                    return null;
                }
            }

            _logger.LogWarning("Exchange: unsupported DID method for {Did}", holderDid);
            return null;
        }

        // Minimal DID document DTO for deserialization.
        private sealed class DidDocumentDto
        {
            [JsonPropertyName("verificationMethod")]
            public List<VerificationMethodDto>? VerificationMethod { get; set; }
        }

        private sealed class VerificationMethodDto
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("publicKeyMultibase")]
            public string? PublicKeyMultibase { get; set; }
        }

        // -----------------------------------------------------------------
        // §Status List  GET /status-lists/1
        // -----------------------------------------------------------------

        /// <summary>
        /// Returns the Bitstring Status List credential so that verifiers (and wallets)
        /// can check whether any issued credential has been revoked.
        ///
        /// The response is a <c>BitstringStatusListCredential</c> per the
        /// <see href="https://www.w3.org/TR/vc-bitstring-status-list/">spec</see>.
        /// It is intentionally returned unsigned in this demo; a production issuer
        /// would sign it with the same key used for individual credentials.
        /// </summary>
        [HttpGet("status-lists/1")]
        public IActionResult GetStatusList()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var statusListCredential = new Dictionary<string, object>
            {
                ["@context"] = new[]
                {
                    "https://www.w3.org/ns/credentials/v2",
                    "https://www.w3.org/ns/credentials/status/v1"
                },
                ["id"]   = _statusListService.StatusListCredentialUrl,
                ["type"] = new[] { "VerifiableCredential", "BitstringStatusListCredential" },
                ["issuer"] = baseUrl,
                ["validFrom"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["credentialSubject"] = new Dictionary<string, object>
                {
                    ["id"]            = _statusListService.StatusListCredentialUrl + "#list",
                    ["type"]          = "BitstringStatusList",
                    ["statusPurpose"] = "revocation",
                    ["encodedList"]   = _statusListService.GetEncodedStatusList()
                }
            };

            return Ok(statusListCredential);
        }

        // -----------------------------------------------------------------
        // DTOs
        // -----------------------------------------------------------------

        /// <summary>
        /// Protocols object returned by the Interaction URL endpoint (§3.7.1).
        /// </summary>
        public sealed class ProtocolsResponse
        {
            /// <summary>VC-API exchange participation URL.</summary>
            [JsonPropertyName("vcapi")]
            public string? Vcapi { get; set; }

            /// <summary>URL where the wallet POSTs its InviteResponse.</summary>
            [JsonPropertyName("inviteRequest")]
            public string? InviteRequest { get; set; }
        }

        /// <summary>InviteResponse body sent by the wallet (§3.7).</summary>
        public sealed class InviteResponseRequest
        {
            [JsonPropertyName("purpose")]
            public string? Purpose { get; set; }

            [JsonPropertyName("referenceId")]
            public string? ReferenceId { get; set; }

            /// <summary>Optional wallet callback URL.</summary>
            [JsonPropertyName("url")]
            public string? Url { get; set; }
        }

        /// <summary>Exchange state DTO returned by GET exchange.</summary>
        public sealed class ExchangeStateResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("state")]
            public string? State { get; set; }

            [JsonPropertyName("sequence")]
            public int Sequence { get; set; }

            [JsonPropertyName("step")]
            public string? Step { get; set; }

            [JsonPropertyName("expires")]
            public string? Expires { get; set; }
        }

        /// <summary>Minimal RFC 7807 Problem Details for error responses.</summary>
        public sealed class ProblemDetailsResponse
        {
            [JsonPropertyName("title")]
            public string Title { get; }

            [JsonPropertyName("status")]
            public int Status { get; }

            public ProblemDetailsResponse(string title, int status)
            {
                Title = title;
                Status = status;
            }
        }
    }
}

