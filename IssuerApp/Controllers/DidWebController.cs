using IssuerApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IssuerApp.Controllers
{
    /// <summary>
    /// Serves did:web DID documents for organizations hosted by IssuerApp.
    ///
    /// Resolution example:
    ///   DID:  did:web:localhost%3A20001:organizations:abc123
    ///   URL:  https://localhost:20001/organizations/abc123/did.json
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    public class DidWebController : ControllerBase
    {
        private readonly IDidWebService _didWebService;

        public DidWebController(IDidWebService didWebService)
        {
            _didWebService = didWebService;
        }

        [HttpGet("organizations/{slug}/did.json")]
        public async Task<IActionResult> GetOrganizationDocument(string slug)
        {
            var document = await _didWebService.GetOrganizationDocumentAsync(slug);
            if (document == null) return NotFound();
            return Ok(document);
        }
    }
}
