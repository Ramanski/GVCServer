using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StationAssistant.Data;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using ModelsLibrary;

namespace StationAssistant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
            private readonly UserManager<IdentityUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly SignInManager<IdentityUser> _signInManager;
            private readonly IConfiguration _configuration;

            public AccountsController(UserManager<IdentityUser> userManager, 
                                      SignInManager<IdentityUser> signInManager,
                                      RoleManager<IdentityRole> roleManager,
                                      IConfiguration configuration)
            {
                _roleManager = roleManager;
                _userManager = userManager;
                _signInManager = signInManager;
                _configuration = configuration;
            }

            [HttpPost("login")]
            public async Task<ActionResult<UserToken>> Login([FromBody] User userInfo)
            {
                var user = await _userManager.FindByNameAsync(userInfo.Login);
                if (user == null) 
                    return BadRequest("Такого логина не существует");
                var singInResult = await _signInManager.CheckPasswordSignInAsync(user, userInfo.Password, false);
                if (!singInResult.Succeeded)
                {
                    return BadRequest("Неверный пароль");
                }
                else
                {
                    var claims = await _userManager.GetClaimsAsync(user);
                    userInfo.Role = claims.Where(cl => cl.Type == ClaimTypes.Role).FirstOrDefault()?.Value;
                    userInfo.Name = claims.Where(cl => cl.Type == ClaimTypes.Name).FirstOrDefault()?.Value;
                    return BuildToken(userInfo);
                }
/*            var result = await _signInManager.PasswordSignInAsync(
                userInfo.Login, userInfo.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                }
                else
                {
                    return BadRequest("Invalid login attempt");
                } */
            }

/*            [HttpGet("roles")]
            public ActionResult<List<IdentityUser>> GetRoles()
            {
                return _roleManager.Roles.ToList();
            }*/


            [HttpPost("create")]
            public async Task<ActionResult<UserToken>> CreateUser([FromBody] User model)
            {
                var user = new IdentityUser() { UserName = model.Login };
                var result = await _userManager.CreateAsync(user, model.Password);

                //await _roleManager.FindByIdAsync()
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, model.Role));
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, model.Name));
                    return BuildToken(model);
                }
                else
                {
                    return BadRequest(result.Errors.FirstOrDefault()?.Description);
                }
            }

            private UserToken BuildToken(User userInfo)
            {
                var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, userInfo.Name == null? "": userInfo.Name),
                        new Claim(ClaimTypes.Role, userInfo.Role)
                    };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.UtcNow.AddDays(1);

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: _configuration["StationCode"],
                    audience: null,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds);

                return new UserToken()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = expiration
                };
            }

            [Authorize]
            [HttpPost]
            public async Task<IActionResult> Logout()
            {
                await _signInManager.SignOutAsync();
                return Ok();
            }
    }
}
