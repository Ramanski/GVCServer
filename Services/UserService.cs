using GVCServer.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GVCServer.Services
{
        public interface IUserService
        {
            User Authenticate(LoginData model);
            IEnumerable<UserDB> GetAll();
            UserDB GetById(int id);
        }

    public class UserDB
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }

        public class UserService : IUserService
        {
            
            // users hardcoded for simplicity, store in a db with hashed passwords in production applications
            private List<UserDB> _users = new List<UserDB>
            {
                new UserDB { Id = 1, Name = "Test from GVC", Login = "gvcmain", Password = "gvcmain", Role = "Admin" }
            };

            private readonly AppSettings _appSettings;

            public UserService(IOptions<AppSettings> appSettings)
            {
                _appSettings = appSettings.Value;
            }

            public User Authenticate(LoginData loginData)
            {
                var userdb = _users.SingleOrDefault(x => x.Login == loginData.Username && x.Password == loginData.Password);

                // return null if user not found
                if (userdb == null) return null;

                User user = new User()
                {
                    FirstName = userdb.Name,
                    Id = userdb.Id,
                    Username = userdb.Login
                };

                // authentication successful so generate jwt token
                var token = generateJwtToken(user);
                user.Token = token;
                return user;
            }

            public IEnumerable<UserDB> GetAll()
            {
                return _users;
            }

            public UserDB GetById(int id)
            {
                return _users.FirstOrDefault(x => x.Id == id);
            }

            // helper methods

            private string generateJwtToken(User user)
            {
                // generate token that is valid for 1 day
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
        }
}
