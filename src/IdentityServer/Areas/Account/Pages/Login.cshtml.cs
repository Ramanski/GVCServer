using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace IdentityServer.Areas.Account.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public string ReturnUrl { get; set; }

            [Display(Name = "Запомнить на этом ПК?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string ReturnUrl)
        {
            _logger.LogDebug("Entered Login page from {ReturnUrl}", ReturnUrl);
            Input = new InputModel();
            Input.ReturnUrl = ReturnUrl;
            
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                _logger.LogWarning("Errors found on getting Login page", ErrorMessage);
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string ReturnUrl = Input?.ReturnUrl ?? Url.Content("~/");
            _logger.LogDebug("Got Login credentials with {ReturnUrl}", ReturnUrl);

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User credentials are correct");
                    _logger.LogDebug("Redirecting to page...", ReturnUrl);
                    return Redirect(ReturnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("User requires Two Factor auth.");
                    _logger.LogDebug("Redirecting...");
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = ReturnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account is locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt", result.ToString());
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            _logger.LogError("Something failed. ModelState is not valid.");
            return Page();
        }
    }
}
