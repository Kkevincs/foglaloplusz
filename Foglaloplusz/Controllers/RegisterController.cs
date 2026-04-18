using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.ArtistDTOs;
using Foglaloplusz.BLL.DTOs.GuestDTOs;
using Foglaloplusz.BLL.DTOs.OwnerDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/register")]
[AllowAnonymous]
[ApiController]
public class RegisterController : ControllerBase
{
    private readonly IOwnerService _ownerService;
    private readonly IArtistService _artistService;
    private readonly IGuestService _guestService;

    public RegisterController(
        IOwnerService ownerService,
        IArtistService artistService,
        IGuestService guestService)
    {
        _ownerService = ownerService;
        _artistService = artistService;
        _guestService = guestService;
    }

    [HttpPost("guest")]
    [SwaggerOperation(Summary = "Vendég regisztráció", Description = "Új vendég fiók létrehozása egyedi e-mail címmel.")]
    [ProducesResponseType(typeof(GuestWithoutIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GuestWithoutIdDTO>> RegisterGuest([FromBody] GuestWithoutIdDTO? user) =>
        Ok(await _guestService.RegisterAsync(user));

    [HttpPost("artist")]
    [SwaggerOperation(Summary = "Szakember regisztráció", Description = "Új szakember fiók létrehozása.")]
    [ProducesResponseType(typeof(ArtistWithoutIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ArtistWithoutIdDTO>> RegisterArtist([FromBody] ArtistWithoutIdDTO? user) =>
        Ok(await _artistService.RegisterAsync(user));

    [HttpPost("owner")]
    [SwaggerOperation(Summary = "Tulajdonos regisztráció", Description = "Új szalon tulajdonos fiók létrehozása.")]
    [ProducesResponseType(typeof(OwnerWithoutIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OwnerWithoutIdDTO>> RegisterOwner([FromBody] OwnerWithoutIdDTO? user) =>
        Ok(await _ownerService.RegisterAsync(user));
}
