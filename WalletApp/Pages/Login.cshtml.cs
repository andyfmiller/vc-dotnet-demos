using WalletApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WalletApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public List<ApplicationUser> Users { get; set; } = [];

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            // Get all users to display for selection
            Users = await _userManager.Users.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Please select a user.");
                Users = await _userManager.Users.ToListAsync();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                Users = await _userManager.Users.ToListAsync();
                return Page();
            }

            // Sign in without password
            await _signInManager.SignInAsync(user, isPersistent: true);

            return LocalRedirect(ReturnUrl ?? "~/");
        }
    }
}