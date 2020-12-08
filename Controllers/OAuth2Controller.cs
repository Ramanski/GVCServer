﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GVCServer.Controllers
{
    [Route("[controller]")]
    public class OAuth2Controller : Controller
    {
        public IConfiguration Configuration { get; }
        public OAuth2Controller(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        [AllowAnonymous]
        [HttpGet("authorize")]
        public IActionResult Authorize(
        string response_type, // authorization flow type 
        string client_id, // client id
        string redirect_uri,
        string scope, // what info I want = email,grandma,tel
        string state) // random string generated to confirm that we are going to back to the same client
        {
            // Todo: Проверка client_id в списке доступных
            Console.WriteLine($"Client {client_id} entered authorized method");

            var query = new QueryBuilder();
            query.Add("redirectUri", redirect_uri);
            query.Add("state", state);

            return RedirectToPage("/Account/Login", new { area = "Identity", redirect_uri = redirect_uri, state = state });
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> Token(
            string grant_type, // flow of access_token request
            string code, // confirmation of the authentication process
            string redirect_uri,
            string client_id,
            string refresh_token) // random string generated to confirm that we are going to back to the same client
        {
            // some mechanism for validating the code

            var claims = new[]
          {
                new Claim(JwtRegisteredClaimNames.Sub, "some_id"),
                new Claim("granny", "cookie")
            };

            var secretBytes = Encoding.UTF8.GetBytes(Configuration["AppSettings:Secret"]);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                Configuration["AppSettings:ServerName"],
                client_id,
                claims,
                notBefore: DateTime.Now,
                expires: grant_type == "refresh_token"
                    ? DateTime.Now.AddMinutes(5)
                    : DateTime.Now.AddDays(10),
                signingCredentials);

            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token,
                token_type = "Bearer",
                raw_claim = "oauthTutorial",
                refresh_token = "RefreshTokenSampleValueSomething77"
            };

            var responseJson = JsonConvert.SerializeObject(responseObject);
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);

            await Response.Body.WriteAsync(responseBytes, 0, responseBytes.Length);

            return Redirect(redirect_uri);
        }

    }
}
