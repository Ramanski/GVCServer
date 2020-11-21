using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GVCServer.Helpers;
using static GVCServer.Services.UserService;
using GVCServer.Services;
using ModelsLibrary;
using Microsoft.AspNetCore.Identity;

namespace GVCServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        //private string station = "161306";
        private IUserService _userService;
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(LoginData loginData)
        {
            var response = _userService.Authenticate(loginData);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

 /*       [HttpPost("register")]
        public async Task<ActionResult<UserToken>> RegisterUser(string station, [FromBody] User model)
        {
            var user = new IdentityUser() { UserName = model.Login };
            var result = await _userManager.CreateAsync(user, model.Password);

            //await _roleManager.FindByIdAsync()
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, model.Role));
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, model.Name));
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.PrimaryGroupSid, station));
                return Ok(model);
            }
            else
            {
                return BadRequest(result.Errors.FirstOrDefault()?.Description);
            }
        }*/

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }
    }
}
