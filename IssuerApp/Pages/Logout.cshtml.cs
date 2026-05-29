using IssuerApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace IssuerApp.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            return Redirect(ReturnUrl ?? "~/");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            return Redirect(ReturnUrl ?? "~/");
        }
    }
}