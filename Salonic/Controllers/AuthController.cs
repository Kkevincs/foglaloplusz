using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/authentication")]
    [AllowAnonymous]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _auth;

        public AuthController(IAuthenticationService auth)
        {
            _auth = auth;
        }

        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<string>> Authenticate([FromBody] UserDTO? authInfo)
        {
            if (authInfo == null)
                return BadRequest("Request body is required.");

            if (string.IsNullOrWhiteSpace(authInfo.Email) ||
                authInfo.Pwd == null ||
                string.IsNullOrWhiteSpace(authInfo.Role))
                return BadRequest("Email, password, and role are required.");

            var role = authInfo.Role.Trim().ToLowerInvariant();
            if (role is not ("guest" or "artist" or "owner"))
                return BadRequest("Role must be guest, artist, or owner.");

            var token = await _auth.SignInAsync(authInfo);
            if (token == null)
                return Unauthorized();

            return Ok(token);
        }
    }
}
