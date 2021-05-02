using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Areas.Account.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IIdentityServerInteractionService _interactionService;

        public LogoutModel(SignInManager<IdentityUser> signInManager, 
                    ILogger<LogoutModel> logger,
                    IIdentityServerInteractionService interactionService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _interactionService = interactionService;
        }

        public async Task<IActionResult> OnGet(string logoutId = null)
        {
            if(logoutId == null)
            {
                _logger.LogDebug("Entered Logout page");
                return Page();
            }
            else
            {
                _logger.LogDebug("Entered Logout page with logoutId");
                await _signInManager.SignOutAsync();

                var logoutRequest = await _interactionService.GetLogoutContextAsync(logoutId);

                if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
                {
                    return Page();
                }

                return Redirect(logoutRequest.PostLogoutRedirectUri);
            }
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                _logger.LogDebug("Local redirect to {returnUrl}", returnUrl);
                return LocalRedirect(returnUrl);
            }
            else
            {
                _logger.LogDebug("Redirecting to root page");
                return RedirectToPage();
            }
        }
    }
}
