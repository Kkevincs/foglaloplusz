using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.BLL.Services.Interfaces;


namespace Salonic.presentation.Controllers
{
    [Route("api/Authentication")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private readonly IAuthenticationService _auth;

        

        public class UserInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public string Role { get; set; }

            public UserInfo(int id, string name, string role)
            {
                Id = id;
                Name = name;
                Role = role;
            }
        }

        public AuthController(IConfiguration configuration, IAuthenticationService auth)
        {
            _configuration = configuration;
            _auth = auth;
        }

        [HttpPost("authenticate")]
        //public ActionResult<string> Authenticate(
        //    AuthenticationInfo authInfo)
        //{
        //    //validate credentials
        //    var user = ValidateCredentials(authInfo.Username, authInfo.Password, authInfo.Role);

        //    //user = null;

        //    if (user == null)
        //    {
        //        return Unauthorized();
        //    }

        //    //need to create the token here
        //    //tokens are signed => we need a key to sign it
        //    //keys are created from secrets => have to store our secret
        //    //=> will store in app settings for development
        //    //need to inject conf into ctor

        //    //actually create the token
        //    var securityKey = new SymmetricSecurityKey(
        //        Convert.FromBase64String(_configuration["Authentication:SecretForKey"]));
        //    //above: pass our secret as a byte array
        //    //now create the signing credentials with our securityKey
        //    var signingCredetials = new SigningCredentials(
        //        securityKey, SecurityAlgorithms.HmacSha256);
        //    //create payload into token
        //    //key-value pairs about the user = claims
        //    var claimsForToken = new List<Claim>()
        //    {
        //        new Claim("sub", user.Id.ToString()),   //sub is the key for user id
        //        new Claim("name", user.Name),
        //        new Claim("role", user.Role)
        //        //other: given_name, family_name
        //    };
        //    //create the token:
        //    var jwtSecurityToken = new JwtSecurityToken(
        //        _configuration["Authentication:Issuer"],
        //        _configuration["Authentication:Audience"],
        //        claimsForToken,
        //        DateTime.UtcNow,                //start of token validity
        //        DateTime.UtcNow.AddHours(1),    //end of token validity
        //        signingCredetials);

        //    var tokenToReturn = new JwtSecurityTokenHandler()
        //        .WriteToken(jwtSecurityToken); //=> this gives the token string we can return

        //    return Ok(tokenToReturn);
        //}

        public async Task<ActionResult<UserInfo>> ValidateCredentials(UserDTO user)
        {
            var authenticatedUser = await _auth.Authentication(user);
            if (!(authenticatedUser == null))
            {
                return Ok(authenticatedUser);
            }
            else {
                return Unauthorized(user);
            }
            

            UserInfo validUser = new(authenticatedUser.Id, authenticatedUser.Name, authenticatedUser.Role);
            if (authenticatedUser != null)
            {

            }
            else {
                return Unauthorized(authenticatedUser);
            }


        }

    }
}
