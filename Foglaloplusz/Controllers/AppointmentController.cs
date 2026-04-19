using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.presentation.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/appointment")]
[ApiController]
public class AppointmentController : ApiControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [Authorize(Roles = "owner")]
    [HttpGet("salon/{salonId:int}")]
    [SwaggerOperation(Summary = "Időpontok listája (tulaj)", Description = "A megadott szalon összes foglalása, ha a tulajdonoshoz tartozik.")]
    [ProducesResponseType(typeof(List<AppointmentsWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetBySalonForOwner(int salonId)
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var list = await _appointmentService.ListForSalonOwnerAsync(ownerId, salonId);
        return Ok(list);
    }

    [Authorize(Roles = "guest")]
    [HttpGet("guest")]
    [SwaggerOperation(Summary = "Vendég időpontjai", Description = "A bejelentkezett vendég foglalásai.")]
    [ProducesResponseType(typeof(List<AppointmentsWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetByGuestId()
    {
        if (!TryGetUserIdFromSub(out var guestId))
            return Unauthorized();
        var list = await _appointmentService.ListForGuestAsync(guestId);
        return Ok(list);
    }

    [Authorize(Roles = "artist")]
    [HttpGet("artist")]
    [SwaggerOperation(Summary = "Szakember időpontjai", Description = "A bejelentkezett szakember foglalásai.")]
    [ProducesResponseType(typeof(List<AppointmentsWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetByArtistId()
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var list = await _appointmentService.ListForArtistAsync(artistId);
        return Ok(list);
    }

    [Authorize(Roles = "guest")]
    [HttpDelete("guest/{id:int}")]
    [SwaggerOperation(Summary = "Időpont lemondása (vendég)", Description = "A saját foglalás törlése.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelForGuest(int id)
    {
        if (!TryGetUserIdFromSub(out var guestId))
            return Unauthorized();
        var ok = await _appointmentService.CancelForGuestAsync(guestId, id);
        if (!ok)
            return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "guest")]
    [HttpPost]
    [SwaggerOperation(Summary = "Új időpont", Description = "Foglalás létrehozása; a vendégazonosítónak egyeznie kell a tokennel.")]
    [ProducesResponseType(typeof(AppointmentsWithoutIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AppointmentsWithoutIdDTO>> Create([FromBody] AppointmentsWithoutIdDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var tokenGuestId))
            return Unauthorized();
        if (dto != null && dto.GuestId != tokenGuestId)
            return Forbid();
        var created = await _appointmentService.CreateAsync(dto);
        return Ok(created);
    }

    [AllowAnonymous]
    [HttpGet("available")]
    [SwaggerOperation(Summary = "Szabad idősávok", Description = "Elérhető kezdő időpontok egy napra (nyilvános). Query: artistId, date (pl. yyyy-MM-dd), serviceIds (többször ismétlhető).")]
    [ProducesResponseType(typeof(List<AvaliableSlotDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<AvaliableSlotDTO>>> GetAvailableSlots([FromQuery] GetAvailableSlotsDTO? request)
    {
        var slots = await _appointmentService.GetAvailableSlotsFromRequestAsync(request);
        return Ok(slots);
    }
}
