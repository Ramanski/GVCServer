using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace StationAssistant.Services
{
    public interface IAuthenticationService
    {
        public Task Login(string username, string password);
        public Task Logout();
        public Task Register(string username, string password);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AuthenticationService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task Login(string username, string password)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                var signInResult = await signInManager.PasswordSignInAsync(user, password, false, false);
                if (signInResult.Succeeded)
                {
                    //
                }
            }
        }

        public async Task Logout()
        {
            await signInManager.SignOutAsync();
        }

        public async Task Register(string username, string password)
        {
            var user = new IdentityUser
            {
                UserName = username,
                Email = ""
            };

            var result = await userManager.CreateAsync(new IdentityUser(username), password);

            if(result.Succeeded)
            {
                await this.Login(username, password);
            }
        }
    }
}
