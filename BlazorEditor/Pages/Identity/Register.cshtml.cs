using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BlazorEditor.Data;
using BlazorEditor.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlazorEditor.Pages.Identity
{
    public class Register : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public Register(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        
        [BindProperty]
        public InputModel Input { get; set; }
        
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }
            
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            
            public string RegisterToken { get; set; }
        }
        
        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            RegisterToken registerToken = await _context.RegisterTokens.FindAsync(Input.RegisterToken);
            bool tokenValid = registerToken != null && DateTime.Now < registerToken.Expire;
            if (!ModelState.IsValid ||
                _userManager.Users.ToList().Count >= 1 && !User.Identity.IsAuthenticated && !tokenValid)
                return Page();
            ApplicationUser user = new ApplicationUser { UserName = Input.Username };
            IdentityResult result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // It can't be null if we've gotten this far
                if (tokenValid)
                {
                    _context.Entry(registerToken).State = EntityState.Deleted;
                    await _context.SaveChangesAsync();
                }
                await _signInManager.SignInAsync(user, false);
                return LocalRedirect(returnUrl);
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}