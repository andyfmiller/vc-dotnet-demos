using Library.Crypto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using VerifierApp.Services;

namespace VerifierApp.Controllers
{
    /// <summary>
    /// Implements the VCALM (VC API for Lifecycle Management) server-side endpoints
    /// required of a Verifier Coordinator.
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
    ///           Wallet sends empty body; verifier responds with a QueryByExample VPR
    ///           containing a one-time challenge.
    ///
    ///   §3.6    POST /workflows/{workflowId}/exchanges/{exchangeId}  (round-trip 2)
    ///           Wallet sends a signed VP containing the holder's credential; verifier
    ///           verifies the VP proof, then verifies the credential proof.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("")]
    public class VcapiController : ControllerBase
    {
        private readonly IVerificationExchangeService _exchangeService;
        private readonly IEd25519SigningService _signingService;
        private readonly IJsonLdCanonicalizationService _canonicalizationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICredentialStatusService _credentialStatusService;
        private readonly ILogger<VcapiController> _logger;

        public VcapiController(
            IVerificationExchangeService exchangeService,
            IEd25519SigningService signingService,
            IJsonLdCanonicalizationService canonicalizationService,
            IHttpClientFactory httpClientFactory,
            ICredentialStatusService credentialStatusService,
            ILogger<VcapiController> logger)
        {
            _exchangeService = exchangeService;
            _signingService = signingService;
            _canonicalizationService = canonicalizationService;
            _httpClientFactory = httpClientFactory;
            _credentialStatusService = credentialStatusService;
            _logger = logger;
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
                Step = "verify",
                Expires = exchange.Expires.ToString("O")
            });
        }

        // -----------------------------------------------------------------
        // §3.6  POST /workflows/{workflowId}/exchanges/{exchangeId}
        // -----------------------------------------------------------------

        /// <summary>
        /// Exchange participation endpoint (Holder Coordinator → Verifier Coordinator).
        ///
        /// QueryByExample verification workflow (two round-trips):
        ///
        ///   Round-trip 1 — wallet POSTs empty body {}:
        ///     Verifier generates a one-time challenge, stores it on the exchange, and
        ///     responds with an ExchangeParticipationServerMessage containing a
        ///     VerifiablePresentationRequest with query type "QueryByExample".
        ///
        ///   Round-trip 2 — wallet POSTs verifiablePresentation:
        ///     Verifier resolves the holder's DID document to obtain the public key,
        ///     verifies the eddsa-rdfc-2022 proof over the VP, then verifies the
        ///     eddsa-rdfc-2022 proof on the enclosed credential and returns a
        ///     verification result.
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
            // → verifier returns a QueryByExample VPR with a fresh challenge.
            // -----------------------------------------------------------------
            if (exchange.Step is null)
            {
                var challenge = Guid.NewGuid().ToString();
                var domain = Request.Host.Value ?? "localhost";
                _exchangeService.StoreChallengeForPresentation(exchangeId, challenge);

                var credentialQuery = new Dictionary<string, object>
                {
                    ["reason"] = exchange.Reason,
                    ["example"] = new Dictionary<string, object>
                    {
                        ["@context"] = new[]
                        {
                            "https://www.w3.org/ns/credentials/v2",
                            "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
                        },
                        ["type"] = new[] { "VerifiableCredential", exchange.CredentialType },
                        ["credentialSubject"] = new Dictionary<string, object>
                        {
                            ["achievement"] = new Dictionary<string, string>
                            {
                                ["achievementType"] = exchange.AchievementType
                            }
                        }
                    },
                    ["acceptedCryptosuites"] = new[]
                    {
                        new Dictionary<string, string> { ["cryptosuite"] = "eddsa-rdfc-2022" }
                    }
                };

                var vpr = new Dictionary<string, object>
                {
                    ["query"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["type"] = "QueryByExample",
                            ["credentialQuery"] = credentialQuery
                        }
                    },
                    ["challenge"] = challenge,
                    ["domain"] = domain
                };

                var serverMessage = new Dictionary<string, object>
                {
                    ["referenceId"] = $"urn:uuid:{Guid.NewGuid()}",
                    ["verifiablePresentationRequest"] = vpr
                };

                _logger.LogInformation(
                    "Exchange {ExchangeId}: Round-trip 1 — QueryByExample VPR sent (challenge={Challenge})",
                    exchangeId, challenge);

                return Ok(serverMessage);
            }

            // -----------------------------------------------------------------
            // Round-trip 2: wallet sends { verifiablePresentation: { … } }
            // → verifier verifies VP proof, then verifies enclosed credential proof.
            // -----------------------------------------------------------------
            if (exchange.Step != "AwaitingPresentation")
            {
                return BadRequest(new ProblemDetailsResponse(
                    $"Unexpected exchange step '{exchange.Step}'.", 400));
            }

            if (!body.HasValue ||
                !body.Value.TryGetProperty("verifiablePresentation", out var vpElement))
            {
                return BadRequest(new ProblemDetailsResponse(
                    "Expected verifiablePresentation in body for presentation step.", 400));
            }

            // ------------------------------------------------------------------
            // Step A: verify the VP proof (proves holder controls the key/DID).
            // ------------------------------------------------------------------
            if (!vpElement.TryGetProperty("proof", out var vpProofEl) ||
                !vpProofEl.TryGetProperty("verificationMethod", out var vmEl) ||
                vmEl.GetString() is not { Length: > 0 } verificationMethod)
            {
                return BadRequest(new ProblemDetailsResponse(
                    "VP proof.verificationMethod is missing or empty.", 400));
            }

            var holderDid = verificationMethod.Contains('#')
                ? verificationMethod[..verificationMethod.IndexOf('#')]
                : verificationMethod;

            var holderPublicKey = await ResolvePublicKeyAsync(holderDid, verificationMethod);
            if (holderPublicKey is null)
            {
                return BadRequest(new ProblemDetailsResponse(
                    $"Could not resolve public key for verificationMethod '{verificationMethod}'.", 400));
            }

            var vpJson = vpElement.GetRawText();
            var vpVerifyResult = VerifyProof(vpJson, exchange.Challenge!, holderPublicKey);
            if (!vpVerifyResult.Success)
            {
                _logger.LogWarning(
                    "Exchange {ExchangeId}: VP proof verification failed — {Reason}",
                    exchangeId, vpVerifyResult.FailureReason);
                return BadRequest(new ProblemDetailsResponse(
                    $"VP proof verification failed: {vpVerifyResult.FailureReason}", 400));
            }

            _exchangeService.StoreHolderDid(exchangeId, holderDid);

            _logger.LogInformation(
                "Exchange {ExchangeId}: VP proof verified. holderDid={HolderDid}",
                exchangeId, holderDid);

            // ------------------------------------------------------------------
            // Step B: extract and verify the enclosed VerifiableCredential proof.
            // ------------------------------------------------------------------
            if (!vpElement.TryGetProperty("verifiableCredential", out var vcArrayEl) ||
                vcArrayEl.ValueKind != JsonValueKind.Array ||
                vcArrayEl.GetArrayLength() == 0)
            {
                _exchangeService.CompleteExchange(
                    exchangeId, passed: false,
                    failureReason: "VP contained no verifiableCredential.",
                    credentialJson: null,
                    proofValid: true);

                return Ok(BuildVerificationResult(false, "VP contained no verifiableCredential."));
            }

            var credentialEl = vcArrayEl[0];
            var credentialJson = JsonSerializer.Serialize(credentialEl,
                new JsonSerializerOptions { WriteIndented = true });

            // Resolve issuer DID and verify credential proof.
            string? issuerDid = null;
            if (credentialEl.TryGetProperty("issuer", out var issuerEl))
            {
                issuerDid = issuerEl.ValueKind == JsonValueKind.String
                    ? issuerEl.GetString()
                    : issuerEl.TryGetProperty("id", out var issuerIdEl)
                        ? issuerIdEl.GetString()
                        : null;
            }

            string? credentialFailureReason = null;
            if (issuerDid is null)
            {
                credentialFailureReason = "Could not determine issuer DID from credential.";
            }
            else if (!credentialEl.TryGetProperty("proof", out var credProofEl) ||
                     !credProofEl.TryGetProperty("verificationMethod", out var credVmEl) ||
                     credVmEl.GetString() is not { Length: > 0 } credVm)
            {
                credentialFailureReason = "Credential proof.verificationMethod is missing or empty.";
            }
            else
            {
                var issuerPublicKey = await ResolvePublicKeyAsync(issuerDid, credVm);
                if (issuerPublicKey is null)
                {
                    credentialFailureReason =
                        $"Could not resolve public key for issuer verificationMethod '{credVm}'.";
                }
                else
                {
                    // Credential proofs use proofPurpose=assertionMethod; no challenge expected.
                    var credVerifyResult = VerifyProof(
                        credentialEl.GetRawText(),
                        expectedChallenge: null,
                        publicKeyMultibase: issuerPublicKey);

                    if (!credVerifyResult.Success)
                        credentialFailureReason = credVerifyResult.FailureReason;
                }
            }

            var passed = credentialFailureReason is null;

            if (!passed)
            {
                _logger.LogWarning(
                    "Exchange {ExchangeId}: Credential proof verification failed — {Reason}",
                    exchangeId, credentialFailureReason);
            }
            else
            {
                _logger.LogInformation(
                    "Exchange {ExchangeId}: Credential proof verified. holderDid={HolderDid}",
                    exchangeId, holderDid);
            }

            // ------------------------------------------------------------------
            // Step C: check live credentialStatus (independent of proof outcome).
            // ------------------------------------------------------------------
            bool? statusValid = null;
            string? statusFailureReason = null;

            var (statusPassed, statusError) =
                await _credentialStatusService.CheckStatusAsync(credentialEl);

            // Only record a status result when the credential contained a credentialStatus entry
            if (credentialEl.TryGetProperty("credentialStatus", out _))
            {
                statusValid = statusPassed;
                statusFailureReason = statusError;

                if (!statusPassed)
                {
                    _logger.LogWarning(
                        "Exchange {ExchangeId}: Credential status check failed — {Reason}",
                        exchangeId, statusError);
                    passed = false;
                }
                else
                {
                    _logger.LogInformation(
                        "Exchange {ExchangeId}: Credential status check passed.",
                        exchangeId);
                }
            }

            _exchangeService.CompleteExchange(
                exchangeId,
                passed: passed,
                failureReason: credentialFailureReason ?? statusFailureReason,
                credentialJson: credentialJson,
                proofValid: credentialFailureReason is null,
                statusValid: statusValid,
                statusFailureReason: statusFailureReason);

            return Ok(BuildVerificationResult(
                passed,
                credentialFailureReason,
                statusValid,
                statusFailureReason));
        }

        // -----------------------------------------------------------------
        // Proof verification
        // -----------------------------------------------------------------

        private record ProofVerifyResult(bool Success, string? FailureReason = null);

        /// <summary>
        /// Verifies the eddsa-rdfc-2022 proof on a JSON document (VP or VC).
        /// When <paramref name="expectedChallenge"/> is non-null the proof's
        /// challenge must match it (used for VP proofs; omit for VC proofs).
        /// </summary>
        private ProofVerifyResult VerifyProof(
            string documentJson,
            string? expectedChallenge,
            string publicKeyMultibase)
        {
            try
            {
                using var doc = JsonDocument.Parse(documentJson);
                var root = doc.RootElement;

                if (!root.TryGetProperty("proof", out var proofEl))
                    return new ProofVerifyResult(false, "Document has no proof.");

                if (expectedChallenge is not null)
                {
                    if (!proofEl.TryGetProperty("challenge", out var challengeEl) ||
                        challengeEl.GetString() != expectedChallenge)
                        return new ProofVerifyResult(
                            false, "proof.challenge does not match issued challenge.");
                }

                if (!proofEl.TryGetProperty("proofValue", out var pvEl) ||
                    pvEl.GetString() is not { Length: > 0 } proofValue)
                    return new ProofVerifyResult(false, "proof.proofValue is missing.");

                // Reconstruct the signed bytes: SHA-256(proofOptions) || SHA-256(docWithoutProof)
                var proofOptions = new Dictionary<string, object?>();
                foreach (var prop in proofEl.EnumerateObject())
                {
                    if (prop.Name == "proofValue") continue;
                    proofOptions[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => (object?)prop.Value.GetString(),
                        JsonValueKind.Array => (object?)prop.Value.EnumerateArray()
                            .Select(e => e.GetString())
                            .ToArray(),
                        _ => (object?)prop.Value.GetRawText()
                    };
                }
                proofOptions["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" };

                var docWithoutProof = new Dictionary<string, object?>();
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name == "proof") continue;
                    docWithoutProof[prop.Name] =
                        JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
                }

                var proofOptionsBytes = _canonicalizationService.Canonicalize(
                    JsonSerializer.Serialize(proofOptions));
                var docBytes = _canonicalizationService.Canonicalize(
                    JsonSerializer.Serialize(docWithoutProof));

                var proofInput = new byte[32 + 32];
                SHA256.HashData(proofOptionsBytes).CopyTo(proofInput, 0);
                SHA256.HashData(docBytes).CopyTo(proofInput, 32);

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
        /// Resolves the public key multibase for a given verificationMethod ID.
        /// Supports did:key (decoded inline) and did:web (fetched via HTTP).
        /// </summary>
        private async Task<string?> ResolvePublicKeyAsync(string did, string verificationMethod)
        {
            if (did.StartsWith("did:key:"))
                return did["did:key:".Length..];

            if (did.StartsWith("did:web:"))
            {
                try
                {
                    var methodSpecific = did["did:web:".Length..];
                    var parts = methodSpecific.Split(':');
                    var host = Uri.UnescapeDataString(parts[0]);
                    var path = parts.Length > 1
                        ? string.Join("/", parts[1..]) + "/did.json"
                        : ".well-known/did.json";
                    var didDocUrl = $"https://{host}/{path}";

                    var client = _httpClientFactory.CreateClient(Constants.HttpClient.Default);
                    var didDoc = await client.GetFromJsonAsync<DidDocumentDto>(didDocUrl);

                    return didDoc?.VerificationMethod?
                        .FirstOrDefault(v => v.Id == verificationMethod)
                        ?.PublicKeyMultibase;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Exchange: failed to resolve DID document for {Did}", did);
                    return null;
                }
            }

            _logger.LogWarning("Exchange: unsupported DID method for {Did}", did);
            return null;
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static Dictionary<string, object> BuildVerificationResult(
            bool passed, string? failureReason,
            bool? statusValid = null, string? statusFailureReason = null)
        {
            var checks = new List<string>();
            var errors = new List<string>();

            if (failureReason is null)
                checks.Add("proof");

            if (statusValid == true)
                checks.Add("credentialStatus");
            else if (statusValid == false && statusFailureReason is not null)
                errors.Add(statusFailureReason);

            if (failureReason is not null)
                errors.Add(failureReason);

            var result = new Dictionary<string, object>
            {
                ["referenceId"] = $"urn:uuid:{Guid.NewGuid()}",
                ["verificationResult"] = new Dictionary<string, object>
                {
                    ["verified"] = passed,
                    ["checks"] = checks.ToArray(),
                    ["errors"] = errors.ToArray()
                }
            };
            return result;
        }

        // -----------------------------------------------------------------
        // DTOs
        // -----------------------------------------------------------------

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

        public sealed class ProtocolsResponse
        {
            [JsonPropertyName("vcapi")]
            public string? Vcapi { get; set; }

            [JsonPropertyName("inviteRequest")]
            public string? InviteRequest { get; set; }
        }

        public sealed class InviteResponseRequest
        {
            [JsonPropertyName("purpose")]
            public string? Purpose { get; set; }

            [JsonPropertyName("referenceId")]
            public string? ReferenceId { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }
        }

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
