using System.Threading.Tasks;
using BlazorEditor.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorEditor.Pages.Identity
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class Logout : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public Logout(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }
        
        public Task<IActionResult> OnGet()
        {
            return OnPost();
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            returnUrl ??= Url.Content("~/");
            return LocalRedirect(returnUrl);
        }
    }
}