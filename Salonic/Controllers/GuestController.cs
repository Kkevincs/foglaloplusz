using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.GuestDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.presentation.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/guest")]
[ApiController]
public class GuestController : ApiControllerBase
{
    private readonly IGuestService _guestService;

    public GuestController(IGuestService guest)
    {
        _guestService = guest;
    }

    [Authorize(Roles = "owner,guest")]
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Vendég profil", Description = "A bejelentkezett vendég adatai (a token „sub” mezője alapján).")]
    [ProducesResponseType(typeof(GuestWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GuestWithIdDTO>> GetById()
    {
        if (!TryGetUserIdFromSub(out var id))
            return Unauthorized();
        var guest = await _guestService.GetMeOrThrowAsync(id, User.IsInRole("guest"));
        return Ok(guest);
    }
}
