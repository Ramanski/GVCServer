using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace StationAssistant.Services
{
    public interface IAuthenticationService
    {
        Task<User> GetUser();
        Task Login(string username, string password);
        Task Logout();
        Task<AuthenticationState> GetAuthenticationStateAsync();
    }

    public class AuthenticationService : AuthenticationStateProvider, IAuthenticationService
    {
        private IHttpService _httpService;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;
        private AuthenticationState Anonymous =>
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public User User { get; private set; }

        public AuthenticationService(
            IHttpService httpService,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService
        ) {
            _httpService = httpService;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
        }

        public async Task<User> GetUser()
        {
            return await _localStorageService.GetItem<User>("user");
        }

        public async Task Login(string username, string password)
        {
            User = await _httpService.Post<User>("/users/authenticate", new { username, password });
            await _localStorageService.SetItem("user", User);
            var authState = BuildAuthenticationState(User.Token);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task Logout()
        {
            User = null;
            await _localStorageService.RemoveItem("user");
            _navigationManager.NavigateTo("login");
            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = (await _localStorageService.GetItem<User>("user"))?.Token;

            if (string.IsNullOrEmpty(token))
            {
                return Anonymous;
            }

            return BuildAuthenticationState(token);
        }

        private AuthenticationState BuildAuthenticationState(string token)
        {
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearder", token);
            var clid = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            return new AuthenticationState(new ClaimsPrincipal(clid));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payLoad = jwt.Split('.')[1];
            //payLoad = payLoad.PadRight(payLoad.Length + payLoad.Length % 4, '=');
            var jsonBytes = ParseBase64WithoutPadding(payLoad);
            //var jsonBytes = Convert.FromBase64String(payLoad);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (roles != null)
            {
                if (roles.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                    foreach (var parseRole in parsedRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parseRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            var output = base64.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding

            switch (base64.Length % 4)
            {
                case 0: break;
                case 2: output += "=="; break;
                case 3: output += "="; break;
                default: throw new ArgumentOutOfRangeException("output", "Illegal base64url string!");
            }

            return Convert.FromBase64String(output);
        }
    }
}