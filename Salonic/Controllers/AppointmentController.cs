using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/appointment")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [Authorize(Roles = "owner")]
        [HttpGet("salon/{salonId:int}")]
        [ProducesResponseType(typeof(List<AppointmentsWithIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetBySalonForOwner(int salonId)
        {
            if (!TryGetIntClaim("sub", out var ownerId))
                return Unauthorized();

            try
            {
                var list = await _appointmentService.ListForSalonOwnerAsync(ownerId, salonId);
                return Ok(list);
            }
            catch (InvalidOperationException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "guest")]
        [HttpGet("guest")]
        [ProducesResponseType(typeof(List<AppointmentsWithIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetByGuestId()
        {
            if (!TryGetIntClaim("sub", out var guestId))
                return Unauthorized();

            var list = await _appointmentService.ListForGuestAsync(guestId);
            return Ok(list);
        }

        [Authorize(Roles = "artist")]
        [HttpGet("artist")]
        [ProducesResponseType(typeof(List<AppointmentsWithIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetByArtistId()
        {
            if (!TryGetIntClaim("sub", out var artistId))
                return Unauthorized();

            try
            {
                var list = await _appointmentService.ListForArtistAsync(artistId);
                return Ok(list);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "guest")]
        [HttpDelete("guest/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelForGuest(int id)
        {
            if (!TryGetIntClaim("sub", out var guestId))
                return Unauthorized();

            var ok = await _appointmentService.CancelForGuestAsync(guestId, id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "guest")]
        [HttpPost]
        [ProducesResponseType(typeof(AppointmentsWithoutIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AppointmentsWithoutIdDTO>> Create([FromBody] AppointmentsWithoutIdDTO dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            if (!TryGetIntClaim("sub", out var tokenGuestId))
                return Unauthorized();

            if (dto.GuestId != tokenGuestId)
                return Forbid();

            try
            {
                var created = await _appointmentService.CreateAsync(dto);
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPatch("available")]
        [ProducesResponseType(typeof(List<AvaliableSlotDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<AvaliableSlotDTO>>> GetAvailableSlots(
            [FromBody] GetAvailableSlotsDTO? request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            try
            {
                var slots = await _appointmentService.GetAvailableSlotsAsync(
                    request.ArtistId,
                    request.ServiceIds?.ToArray() ?? Array.Empty<int>(),
                    request.Date);
                return Ok(slots);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool TryGetIntClaim(string type, out int value)
        {
            value = 0;
            var raw = User.FindFirst(type)?.Value;
            return raw != null && int.TryParse(raw, out value);
        }
    }
}
