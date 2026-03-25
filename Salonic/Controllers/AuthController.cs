using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.BLL.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;


namespace Salonic.presentation.Controllers
{
    [Route("api/Authentication")]
    [AllowAnonymous]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        private readonly IConfiguration _configuration;

        private readonly IAuthenticationService _auth;

        

        public class UserInfo
        {
            public int Id { get; set; }
            public string Email { get; set; }

            public string Role { get; set; }

            public UserInfo(int id, string email, string role)
            {
                Id = id;
                Email = email;
                Role = role;
            }
        }

        public AuthController(IConfiguration configuration, IAuthenticationService auth)
        {
            _configuration = configuration;
            _auth = auth;
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<string>> Authenticate(
            UserDTO authInfo)
        {
          
            var user = await ValidateCredentials(authInfo);

           

            if (user == null)
            {
                return Unauthorized();
            }

          
            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_configuration["Authentication:SecretForKey"]));
           
            var signingCredetials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);
            
            var claimsForToken = new List<Claim>()
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("role", user.Role)
                
            };
            
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,               
                DateTime.UtcNow.AddHours(1),    
                signingCredetials);

            var tokenToReturn = new JwtSecurityTokenHandler()
                .WriteToken(jwtSecurityToken);

            return Ok(tokenToReturn);
        }

        private async Task<UserInfo> ValidateCredentials(UserDTO user)
        {
            
            var authenticatedUser = await _auth.Authentication(user);
            if (!(authenticatedUser == null))
            {

                UserInfo validUser = new UserInfo(authenticatedUser.Id, authenticatedUser.Email, authenticatedUser.Role);

                return validUser;
            }
            else {
                return null;
            }

        }
    }
}
