using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace IssuerApp.Services
{
    /// <summary>
    /// In-memory <see cref="IStatusListService"/> implementation.
    ///
    /// The status list is a byte array of <see cref="MinimumSize"/> bits initialised to 0
    /// (all credentials active).  Each credential occupies exactly one bit at its
    /// <c>statusListIndex</c>.  The bit is set to 1 when the credential is revoked.
    ///
    /// The list is serialised as:
    ///   1. GZIP-compress the raw byte array.
    ///   2. Base64url-encode without padding — the <c>encodedList</c> value.
    ///
    /// Registered as a singleton so the list survives across requests.
    /// </summary>
    public class StatusListService : IStatusListService
    {
        /// <summary>
        /// Minimum bitstring size required by the spec (131,072 bits = 16 KB).
        /// </summary>
        private const int MinimumSize = 131_072;

        private readonly byte[] _bits;
        private int _nextIndex;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly object _lock = new();

        public StatusListService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _bits = new byte[MinimumSize / 8];
        }

        // -----------------------------------------------------------------
        // URL helpers — derived lazily from the current request context so
        // that the service works regardless of the host / port / base-path.
        // -----------------------------------------------------------------

        private string BaseUrl
        {
            get
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx is null) return "https://localhost";
                return $"{ctx.Request.Scheme}://{ctx.Request.Host}{ctx.Request.PathBase}";
            }
        }

        public string StatusListCredentialUrl => $"{BaseUrl}/status-lists/1";
        public string StatusListEntryBaseUrl  => $"{BaseUrl}/status-lists/1#revocation";

        // -----------------------------------------------------------------
        // IStatusListService
        // -----------------------------------------------------------------

        public int AllocateIndex()
        {
            lock (_lock)
            {
                return _nextIndex++;
            }
        }

        public void SetStatus(int index, bool revoked)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            var byteIndex = index / 8;
            var bitMask   = (byte)(1 << (index % 8));

            lock (_lock)
            {
                if (index >= _bits.Length * 8)
                    throw new ArgumentOutOfRangeException(nameof(index),
                        $"Index {index} exceeds status list capacity {_bits.Length * 8}.");

                if (revoked)
                    _bits[byteIndex] |= bitMask;
                else
                    _bits[byteIndex] &= (byte)~bitMask;
            }
        }

        public bool IsRevoked(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            if (index >= _bits.Length * 8) return false;

            lock (_lock)
            {
                var byteIndex = index / 8;
                var bitMask   = (byte)(1 << (index % 8));
                return (_bits[byteIndex] & bitMask) != 0;
            }
        }

        public string GetEncodedStatusList()
        {
            byte[] snapshot;
            lock (_lock)
            {
                snapshot = (byte[])_bits.Clone();
            }

            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize))
            {
                gzip.Write(snapshot, 0, snapshot.Length);
            }

            // Base64url without padding (spec §4.1)
            return Convert.ToBase64String(output.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
