using System.IO.Compression;
using System.Text.Json;
using VerifierApp.Services;

namespace VerifierApp.Services
{
    /// <summary>
    /// Fetches the issuer's Bitstring Status List credential and checks whether the
    /// credential is revoked by reading the bit at <c>statusListIndex</c>.
    ///
    /// Only <c>BitstringStatusListEntry</c> entries are processed; any other
    /// <c>credentialStatus</c> types are silently skipped (open-world principle).
    /// </summary>
    public class CredentialStatusService : ICredentialStatusService
    {
        private const string BitstringStatusListEntryType = "BitstringStatusListEntry";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CredentialStatusService> _logger;

        public CredentialStatusService(
            IHttpClientFactory httpClientFactory,
            ILogger<CredentialStatusService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<(bool Passed, string? FailureReason)> CheckStatusAsync(
            JsonElement credentialElement,
            CancellationToken cancellationToken = default)
        {
            if (!credentialElement.TryGetProperty("credentialStatus", out var statusEl))
                return (true, null); // no credentialStatus → not revoked

            // credentialStatus can be a single object or an array
            var entries = statusEl.ValueKind == JsonValueKind.Array
                ? statusEl.EnumerateArray().ToList()
                : [statusEl];

            foreach (var entry in entries)
            {
                if (!entry.TryGetProperty("type", out var typeEl) ||
                    typeEl.GetString() != BitstringStatusListEntryType)
                    continue;

                if (!entry.TryGetProperty("statusListCredential", out var listUrlEl) ||
                    listUrlEl.GetString() is not { Length: > 0 } listUrl)
                {
                    return (false, "credentialStatus entry is missing statusListCredential URL.");
                }

                if (!entry.TryGetProperty("statusListIndex", out var indexEl))
                {
                    return (false, "credentialStatus entry is missing statusListIndex.");
                }

                // statusListIndex may be a number or a quoted number string
                int statusListIndex;
                if (indexEl.ValueKind == JsonValueKind.Number)
                {
                    statusListIndex = indexEl.GetInt32();
                }
                else if (indexEl.ValueKind == JsonValueKind.String &&
                         int.TryParse(indexEl.GetString(), out var parsed))
                {
                    statusListIndex = parsed;
                }
                else
                {
                    return (false, $"credentialStatus.statusListIndex has unexpected value '{indexEl}'.");
                }

                // Fetch the status list credential from the issuer
                byte[]? bits;
                try
                {
                    bits = await FetchStatusListBitsAsync(listUrl, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch status list from {Url}", listUrl);
                    return (false, $"Could not fetch status list credential from '{listUrl}': {ex.Message}");
                }

                if (bits is null)
                    return (false, $"Status list credential at '{listUrl}' did not contain a valid encodedList.");

                // Check the bit at statusListIndex
                var byteIndex = statusListIndex / 8;
                if (byteIndex >= bits.Length)
                    return (false, $"statusListIndex {statusListIndex} is out of range for the fetched status list.");

                var bitMask = (byte)(1 << (statusListIndex % 8));
                var isRevoked = (bits[byteIndex] & bitMask) != 0;

                if (isRevoked)
                {
                    _logger.LogInformation(
                        "Credential status check: REVOKED (statusListIndex={Index}, list={Url})",
                        statusListIndex, listUrl);
                    return (false, $"Credential has been revoked (statusListIndex={statusListIndex}).");
                }

                _logger.LogInformation(
                    "Credential status check: ACTIVE (statusListIndex={Index}, list={Url})",
                    statusListIndex, listUrl);
            }

            return (true, null);
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        /// <summary>
        /// GETs the Bitstring Status List credential, extracts <c>encodedList</c>
        /// from <c>credentialSubject</c>, and decodes it back to a raw bit array.
        /// </summary>
        private async Task<byte[]?> FetchStatusListBitsAsync(
            string statusListCredentialUrl,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClient.Default);
            var response = await client.GetAsync(statusListCredentialUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            // Walk: credential → credentialSubject → encodedList
            if (!doc.RootElement.TryGetProperty("credentialSubject", out var subjectEl))
                return null;

            if (!subjectEl.TryGetProperty("encodedList", out var encodedListEl) ||
                encodedListEl.GetString() is not { Length: > 0 } encodedList)
                return null;

            // Reverse base64url → base64 → decode → gunzip
            var base64 = encodedList.Replace('-', '+').Replace('_', '/');
            // Add padding if needed
            base64 = (base64.Length % 4) switch
            {
                2 => base64 + "==",
                3 => base64 + "=",
                _ => base64
            };

            var compressed = Convert.FromBase64String(base64);

            using var compressedStream = new MemoryStream(compressed);
            using var gzip = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var output = new MemoryStream();
            await gzip.CopyToAsync(output, cancellationToken);
            return output.ToArray();
        }
    }
}
