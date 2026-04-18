using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.OwnerDTOs;
using Foglaloplusz.BLL.DTOs.UserDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.presentation.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/owner")]
[ApiController]
public class OwnerController : ApiControllerBase
{
    private readonly IOwnerService _ownerService;

    public OwnerController(IOwnerService owner)
    {
        _ownerService = owner;
    }

    [Authorize(Roles = "owner")]
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Tulajdonos profil", Description = "A bejelentkezett tulajdonos adatai.")]
    [ProducesResponseType(typeof(OwnerWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OwnerWithIdDTO>> GetMe()
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var owner = await _ownerService.GetMeOrThrowAsync(ownerId);
        return Ok(owner);
    }

    [Authorize(Roles = "owner")]
    [HttpPut("me")]
    [SwaggerOperation(Summary = "Tulajdonos profil módosítása", Description = "Név, e-mail, opcionális jelszócsere.")]
    [ProducesResponseType(typeof(OwnerWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OwnerWithIdDTO>> UpdateMe([FromBody] UserProfileUpdateDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var updated = await _ownerService.UpdateProfileAsync(ownerId, dto!);
        return Ok(updated);
    }
}
