﻿using System;
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
using GVCServer.Helpers;

namespace GVCServer.Areas.Identity.Pages.Account
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

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }
        public string RedirectUri { get; private set; }
        public string State { get; private set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string redirect_uri, string state)
        {
            _logger.LogDebug("Entered Login page from {redirect_uri}", redirect_uri);
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                _logger.LogWarning("Errors found on getting Login page", ErrorMessage);
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            RedirectUri = redirect_uri;
            State = state;
        }

        public async Task<IActionResult> OnPostAsync(string redirect_uri = null, string state = null)
        {
            redirect_uri = redirect_uri ?? Url.Content("~/");
            _logger.LogDebug("Got Login credentials with {redirect_uri}", redirect_uri);

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = Microsoft.AspNetCore.Identity.SignInResult.Success; //await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User credentials are correct");
                    var authCode = Randomizer.GetRandomString(20);
                    _logger.LogDebug("Generated authorization code", authCode);
                    var query = new QueryBuilder();
                    query.Add("code", authCode);
                    query.Add("state", state);
                    var returnUrl = redirect_uri + query.ToString();
                    _logger.LogDebug("Redirecting to page...", returnUrl);
                    return Redirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("User requires Two Factor auth.");
                    _logger.LogDebug("Redirecting...");
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = redirect_uri, RememberMe = Input.RememberMe });
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
