using Library.DidWeb;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WalletApp.Data;

namespace WalletApp.Services
{
    public interface IDidWebHolderService
    {
        /// <summary>
        /// Returns the current HTTP request host, falling back to the configured default.
        /// </summary>
        string GetCurrentHost();

        /// <summary>
        /// Builds a did:web identifier for a holder.
        /// Format: did:web:{encodedHost}:holders:{slug}
        /// </summary>
        string BuildHolderDid(string host, string slug);

        /// <summary>
        /// Looks up the DID document for a holder identified by <paramref name="slug"/>.
        /// </summary>
        Task<DidDocument?> GetHolderDocumentAsync(string slug);
    }

    public class DidWebHolderService : IDidWebHolderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<DidWebOptions> _options;
        private readonly ApplicationDbContext _context;

        public DidWebHolderService(
            IHttpContextAccessor httpContextAccessor,
            IOptions<DidWebOptions> options,
            ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options;
            _context = context;
        }

        public string GetCurrentHost()
            => _httpContextAccessor.HttpContext?.Request.Host.Value ?? _options.Value.DefaultHost;

        public string BuildHolderDid(string host, string slug)
        {
            var encodedHost = host.Replace(":", "%3A");
            return $"did:web:{encodedHost}:holders:{slug}";
        }

        public async Task<DidDocument?> GetHolderDocumentAsync(string slug)
        {
            var holder = await _context.Holders
                .FirstOrDefaultAsync(h => h.Id.EndsWith(":holders:" + slug));

            if (holder == null) return null;

            var vmId = $"{holder.Id}#key-1";

            var vm = new VerificationMethod
            {
                Id = vmId,
                Type = "Ed25519VerificationKey2020",
                Controller = holder.Id,
                PublicKeyMultibase = holder.SigningPublicKeyMultibase
            };

            return new DidDocument
            {
                Id = holder.Id,
                VerificationMethod = [vm],
                Authentication = [vmId],
                AssertionMethod = [vmId]
            };
        }
    }
}
