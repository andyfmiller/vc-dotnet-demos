using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WalletApp.Services;

namespace WalletApp.Controllers
{
    /// <summary>
    /// Serves did:web DID documents for holders hosted by WalletApp.
    ///
    /// Resolution example:
    ///   DID:  did:web:localhost%3A22001:holders:abc123
    ///   URL:  https://localhost:22001/holders/abc123/did.json
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    public class DidWebController : ControllerBase
    {
        private readonly IDidWebHolderService _didWebService;

        public DidWebController(IDidWebHolderService didWebService)
        {
            _didWebService = didWebService;
        }

        [HttpGet("holders/{slug}/did.json")]
        public async Task<IActionResult> GetHolderDocument(string slug)
        {
            var document = await _didWebService.GetHolderDocumentAsync(slug);
            if (document == null) return NotFound();
            return Ok(document);
        }
    }
}
