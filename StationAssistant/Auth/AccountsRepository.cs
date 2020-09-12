using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using StationAssistant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ModelsLibrary;

namespace StationAssistant.Auth
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly HttpClient _client;
        private readonly string baseURL = "api/Accounts";

        public AccountsRepository(IHttpContextAccessor httpContext)
        {
            var appBaseUrl = $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}{httpContext.HttpContext.Request.PathBase}";
            _client = new HttpClient() { BaseAddress = new Uri(appBaseUrl) }; /*(HttpClient)provider.GetService(typeof(HttpClient));*/
        }

        public async Task<UserToken> Regiser(User userInfo)
        {
            var response = await _client.PostAsJsonAsync($"{baseURL}/create", userInfo);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) 
                throw new Exception(await response.Content.ReadAsStringAsync());

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(response.StatusCode.ToString());
            return await response.Content.ReadFromJsonAsync<UserToken>();
        }

        public async Task<UserToken> Login(User userInfo)
        {
            var response = await _client.PostAsJsonAsync($"{baseURL}/login", userInfo);
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception(await response.Content.ReadAsStringAsync());
            
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(response.StatusCode.ToString());
            return await response.Content.ReadFromJsonAsync<UserToken>();
        }
    }
}
