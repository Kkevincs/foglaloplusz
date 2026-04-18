using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.UserDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

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

    /// <summary>Bejelentkezés JWT tokenért (érvénytelen adatoknál 400, hibás jelszónál 401).</summary>
    [HttpPost("authenticate")]
    [SwaggerOperation(Summary = "Bejelentkezés", Description = "Visszaad egy JWT tokent, ha az e-mail, jelszó és szerepkör helyes.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> Authenticate([FromBody] UserDTO? authInfo)
    {
        var token = await _auth.SignInAsync(authInfo);
        if (token == null)
            return Unauthorized();
        return Ok(token);
    }
}
