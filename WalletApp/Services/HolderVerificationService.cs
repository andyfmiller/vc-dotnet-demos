using Library.Crypto;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using WalletApp.Data.Models;

namespace WalletApp.Services
{
    /// <summary>
    /// Implements the Holder Coordinator role (§3.6, §3.7) for a VCALM verification
    /// exchange by making real HTTP calls to the VerifierApp's VCALM endpoints.
    ///
    /// Full two-round-trip flow:
    ///   Round-trip 1: POST {} → receive QueryByExample VPR with challenge.
    ///   Round-trip 2: Sign a VP containing the holder's credential with the holder's
    ///                 Ed25519 key and POST it back. Receive the verificationResult.
    /// </summary>
    public class HolderVerificationService : IHolderVerificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEd25519SigningService _signingService;
        private readonly IJsonLdCanonicalizationService _canonicalizationService;
        private readonly ILogger<HolderVerificationService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public HolderVerificationService(
            IHttpClientFactory httpClientFactory,
            IEd25519SigningService signingService,
            IJsonLdCanonicalizationService canonicalizationService,
            ILogger<HolderVerificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _signingService = signingService;
            _canonicalizationService = canonicalizationService;
            _logger = logger;
        }

        public async Task<HolderVerificationResult> PresentCredentialAsync(
            string interactionUrl,
            Holder holder,
            HolderCredential credential)
        {
            var request = await GetPresentationRequestAsync(interactionUrl);
            if (!request.Success)
                return new HolderVerificationResult { Success = false, ErrorMessage = request.ErrorMessage };

            return await PresentCredentialsAsync(request, holder, [credential]);
        }

        public async Task<InteractionRequestResult> GetPresentationRequestAsync(string interactionUrl)
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
                    return FailRequest($"GET {interactionUrl} returned {(int)protocolsResponse.StatusCode}");

                var protocols = await protocolsResponse.Content
                    .ReadFromJsonAsync<ProtocolsDto>(_jsonOptions);

                if (string.IsNullOrWhiteSpace(protocols?.Vcapi))
                    return FailRequest("Protocols response did not contain a 'vcapi' exchange URL.");

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
                            Purpose = "AchievementCredential verification",
                            ReferenceId = $"urn:uuid:{Guid.NewGuid()}"
                        });

                    _logger.LogInformation(
                        "Step 2: InviteResponse status = {Status}", (int)inviteResult.StatusCode);
                }

                // ---------------------------------------------------------------
                // Step 3 (Round-trip 1): POST {} → receive QueryByExample VPR
                // ---------------------------------------------------------------
                _logger.LogInformation("Step 3: POST {{}} to exchange URL {Url}", protocols.Vcapi);

                var rt1Result = await client.PostAsJsonAsync(protocols.Vcapi, new { });
                if (!rt1Result.IsSuccessStatusCode)
                {
                    var err = await rt1Result.Content.ReadAsStringAsync();
                    return FailRequest($"Round-trip 1 returned {(int)rt1Result.StatusCode}: {err}");
                }

                var rt1Json = await rt1Result.Content.ReadAsStringAsync();
                _logger.LogInformation(
                    "Step 3: Round-trip 1 response received ({Bytes} bytes).", rt1Json.Length);

                // Parse the challenge, domain, and QBE types from the VPR.
                using var rt1Doc = JsonDocument.Parse(rt1Json);
                if (!rt1Doc.RootElement.TryGetProperty("verifiablePresentationRequest", out var vprEl))
                    return FailRequest("Round-trip 1 response did not contain a verifiablePresentationRequest.");

                if (!vprEl.TryGetProperty("challenge", out var challengeEl) ||
                    challengeEl.GetString() is not { Length: > 0 } challenge)
                    return FailRequest("VPR did not contain a challenge.");

                vprEl.TryGetProperty("domain", out var domainEl);
                var domain = domainEl.GetString() ?? string.Empty;

                // Extract required credential types and achievement types from the QBE.
                var credentialTypes = ExtractQbeCredentialTypes(vprEl);
                var achievementTypes = ExtractQbeAchievementTypes(vprEl);

                _logger.LogInformation(
                    "Step 3: QBE VPR received. challenge={Challenge} domain={Domain} " +
                    "credentialTypes=[{CredTypes}] achievementTypes=[{AchTypes}]",
                    challenge, domain,
                    string.Join(", ", credentialTypes),
                    string.Join(", ", achievementTypes));

                return new InteractionRequestResult
                {
                    Success = true,
                    ExchangeUrl = protocols.Vcapi,
                    Challenge = challenge,
                    Domain = domain,
                    RequiredCredentialTypes = credentialTypes,
                    RequiredAchievementTypes = achievementTypes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during presentation request for URL {Url}", interactionUrl);
                return FailRequest($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<HolderVerificationResult> PresentCredentialsAsync(
            InteractionRequestResult request,
            Holder holder,
            IEnumerable<HolderCredential> credentials)
        {
            if (!request.Success || string.IsNullOrWhiteSpace(request.ExchangeUrl))
                return Fail("Invalid presentation request.");

            var client = _httpClientFactory.CreateClient(Constants.HttpClient.Default);

            try
            {
                var challenge = request.Challenge!;
                var domain = request.Domain ?? string.Empty;

                // ---------------------------------------------------------------
                // Step 4 (Round-trip 2): Build and sign a VP, then POST it back.
                // ---------------------------------------------------------------
                if (string.IsNullOrWhiteSpace(holder.SigningPrivateKeyBase64))
                    return Fail(
                        "Cannot complete verification: no signing key available on the holder. " +
                        "Ensure the selected holder has a generated key pair.");

                var vmId = $"{holder.Id}#key-1";

                // Parse each credential JSON so it can be embedded as a JSON object.
                var credentialElements = credentials
                    .Select(c =>
                    {
                        using var doc = JsonDocument.Parse(c.CredentialJson);
                        return doc.RootElement.Clone();
                    })
                    .ToArray();

                // Build the VP (without proof).
                var vpWithoutProof = new Dictionary<string, object>
                {
                    ["@context"] = new[] { "https://www.w3.org/ns/credentials/v2" },
                    ["type"] = new[] { "VerifiablePresentation" },
                    ["holder"] = holder.Id,
                    ["verifiableCredential"] = credentialElements
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

                // Attach the proof.
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
                    "Step 4: Posting VP with {Count} credential(s) (holder={HolderDid}, vm={Vm})",
                    credentialElements.Length, holder.Id, vmId);

                var rt2Body = new Dictionary<string, object> { ["verifiablePresentation"] = vpWithProof };
                var rt2Result = await client.PostAsJsonAsync(request.ExchangeUrl, rt2Body);

                var serverMessageJson = await rt2Result.Content.ReadAsStringAsync();

                if (!rt2Result.IsSuccessStatusCode)
                    return Fail($"Round-trip 2 returned {(int)rt2Result.StatusCode}: {serverMessageJson}");

                _logger.LogInformation(
                    "Step 4: Exchange complete. Server message received ({Bytes} bytes).",
                    serverMessageJson.Length);

                // ---------------------------------------------------------------
                // Step 5: Parse verificationResult from the server message.
                // ---------------------------------------------------------------
                bool? verified = null;
                string[]? errors = null;

                try
                {
                    using var doc = JsonDocument.Parse(serverMessageJson);
                    if (doc.RootElement.TryGetProperty("verificationResult", out var vrEl))
                    {
                        if (vrEl.TryGetProperty("verified", out var verifiedEl))
                            verified = verifiedEl.GetBoolean();

                        if (vrEl.TryGetProperty("errors", out var errorsEl) &&
                            errorsEl.ValueKind == JsonValueKind.Array)
                        {
                            errors = errorsEl.EnumerateArray()
                                .Select(e => e.GetString() ?? string.Empty)
                                .Where(s => s.Length > 0)
                                .ToArray();
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Could not parse verificationResult from server message.");
                }

                return new HolderVerificationResult
                {
                    Success = true,
                    ServerMessageJson = serverMessageJson,
                    Verified = verified,
                    VerifierErrors = errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during credential presentation for exchange URL {Url}",
                    request.ExchangeUrl);
                return Fail($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts credential types from the QueryByExample's query[].credentialQuery.example.type arrays.
        /// Excludes "VerifiableCredential".
        /// </summary>
        private static string[] ExtractQbeCredentialTypes(JsonElement vprEl)
        {
            var types = new List<string>();
            if (!vprEl.TryGetProperty("query", out var queryArr) ||
                queryArr.ValueKind != JsonValueKind.Array)
                return [];

            foreach (var q in queryArr.EnumerateArray())
            {
                if (!q.TryGetProperty("credentialQuery", out var cq))
                    continue;

                // credentialQuery may be a single object or an array of objects.
                IEnumerable<JsonElement> cqItems = cq.ValueKind == JsonValueKind.Array
                    ? cq.EnumerateArray()
                    : [cq];

                foreach (var item in cqItems)
                {
                    // Types are nested inside "example".
                    if (!item.TryGetProperty("example", out var example))
                        continue;

                    if (!example.TryGetProperty("type", out var typeArr) ||
                        typeArr.ValueKind != JsonValueKind.Array)
                        continue;

                    foreach (var t in typeArr.EnumerateArray())
                    {
                        var s = t.GetString();
                        if (s is not null && s != "VerifiableCredential")
                            types.Add(s);
                    }
                }
            }

            return [.. types.Distinct()];
        }

        /// <summary>
        /// Extracts achievement types from the QueryByExample's
        /// credentialQuery.example.credentialSubject.achievement.achievementType.
        /// </summary>
        private static string[] ExtractQbeAchievementTypes(JsonElement vprEl)
        {
            var types = new List<string>();
            if (!vprEl.TryGetProperty("query", out var queryArr) ||
                queryArr.ValueKind != JsonValueKind.Array)
                return [];

            foreach (var q in queryArr.EnumerateArray())
            {
                if (!q.TryGetProperty("credentialQuery", out var cq))
                    continue;

                // credentialQuery may be a single object or an array of objects.
                IEnumerable<JsonElement> cqItems = cq.ValueKind == JsonValueKind.Array
                    ? cq.EnumerateArray()
                    : [cq];

                foreach (var item in cqItems)
                {
                    // Achievement type is nested inside example.credentialSubject.achievement.
                    if (!item.TryGetProperty("example", out var example))
                        continue;

                    if (!example.TryGetProperty("credentialSubject", out var subj))
                        continue;

                    if (!subj.TryGetProperty("achievement", out var ach))
                        continue;

                    // The verifier stores it as "achievementType" (a string).
                    if (ach.TryGetProperty("achievementType", out var achTypeEl) &&
                        achTypeEl.ValueKind == JsonValueKind.String)
                    {
                        var s = achTypeEl.GetString();
                        if (s is not null)
                            types.Add(s);
                    }
                }
            }

            return [.. types.Distinct()];
        }

        private static HolderVerificationResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };

        private static InteractionRequestResult FailRequest(string message) =>
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
        }
    }
}
