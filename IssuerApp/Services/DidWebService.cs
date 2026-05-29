using IssuerApp.Data;
using Library.DidWeb;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IssuerApp.Services
{
    public interface IDidWebService
    {
        /// <summary>
        /// Returns the current HTTP request host, falling back to the configured default.
        /// </summary>
        string GetCurrentHost();

        /// <summary>
        /// Builds a did:web identifier for an organization.
        /// Format: did:web:{encodedHost}:organizations:{slug}
        /// </summary>
        string BuildOrganizationDid(string host, string slug);

        /// <summary>
        /// Looks up the DID document for an organization identified by <paramref name="slug"/>.
        /// </summary>
        Task<DidDocument?> GetOrganizationDocumentAsync(string slug);
    }

    public class DidWebService : IDidWebService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<DidWebOptions> _options;
        private readonly ApplicationDbContext _context;

        public DidWebService(
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

        public string BuildOrganizationDid(string host, string slug)
        {
            // Per did:web spec, colons in the host must be percent-encoded
            var encodedHost = host.Replace(":", "%3A");
            return $"did:web:{encodedHost}:organizations:{slug}";
        }

        public async Task<DidDocument?> GetOrganizationDocumentAsync(string slug)
        {
            // Match on the slug (last colon-delimited segment of the DID)
            var org = await _context.Organizations
                .Include(o => o.Profile)
                .FirstOrDefaultAsync(o => o.Profile != null &&
                                          o.Profile.Id.EndsWith(":organizations:" + slug));

            if (org?.Profile == null) return null;

            var did = org.Profile.Id;
            var verificationMethods = new List<VerificationMethod>();
            var assertionMethods = new List<string>();

            if (!string.IsNullOrEmpty(org.SigningPublicKeyMultibase))
            {
                var vmId = $"{did}#key-1";
                verificationMethods.Add(new VerificationMethod
                {
                    Id = vmId,
                    Type = "Ed25519VerificationKey2020",
                    Controller = did,
                    PublicKeyMultibase = org.SigningPublicKeyMultibase
                });
                assertionMethods.Add(vmId);
            }

            return new DidDocument
            {
                Id = did,
                VerificationMethod = verificationMethods,
                AssertionMethod = assertionMethods,
                Authentication = assertionMethods
            };
        }
    }
}
