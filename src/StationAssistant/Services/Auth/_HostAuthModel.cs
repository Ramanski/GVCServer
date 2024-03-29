﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StationAssistant;

namespace StationAssistant.Pages
    {
        public class _HostAuthModel : PageModel
        {
            public readonly BlazorServerAuthStateCache Cache;

            public _HostAuthModel(BlazorServerAuthStateCache cache)
            {
                Cache = cache;
            }

            public async Task<IActionResult> OnGet()
            {
                System.Diagnostics.Debug.WriteLine($"\n_Host OnGet IsAuth? {User.Identity.IsAuthenticated}");

                if (User.Identity.IsAuthenticated)
                {
                    var sid = User.Claims
                        .Where(c => c.Type.Equals("sid"))
                        .Select(c => c.Value)
                        .FirstOrDefault();

                    System.Diagnostics.Debug.WriteLine($"sid: {sid}");

                    if (sid != null && !Cache.HasSubjectId(sid))
                    {
                        var authResult = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
                        DateTimeOffset expiration = authResult.Properties.ExpiresUtc.Value;
                        string accessToken = await HttpContext.GetTokenAsync("access_token");
                        string refreshToken = await HttpContext.GetTokenAsync("refresh_token");
                        Cache.Add(sid, expiration, accessToken, refreshToken);
                    }
                }
                return Page();
            }

            public IActionResult OnGetLogin(string redirectUri = null)
            {
                System.Diagnostics.Debug.WriteLine("\n_Host OnGetLogin");
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(10),
                    //ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(30),
                    RedirectUri = string.IsNullOrEmpty(redirectUri) ? Url.Content("~/") : redirectUri,
                };
                return Challenge(authProps, OpenIdConnectDefaults.AuthenticationScheme);
            }

            public async Task OnGetLogout()
            {
                System.Diagnostics.Debug.WriteLine("\n_Host OnGetLogout");
                var authProps = new AuthenticationProperties
                {
                    RedirectUri = Url.Content("~/")
                };
                await HttpContext.SignOutAsync("AppCookie");
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, authProps);
            }
        }
    }
