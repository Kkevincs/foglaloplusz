using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Services.Interfaces;

namespace Foglaloplusz.presentation.Controllers
{
    [Route("api/workinghours")]
    [ApiController]
    public class WorkingHoursController : ControllerBase
    {
        private readonly IWorkingHoursService _workingHoursService;

        public WorkingHoursController(IWorkingHoursService workingHoursService)
        {
            _workingHoursService = workingHoursService;
        }

        [Authorize(Roles = "artist")]
        [HttpPost]
        [ProducesResponseType(typeof(WorkingHoursDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<WorkingHoursDTO>> SetWorkingHours([FromBody] WorkingHoursDTO? dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                await _workingHoursService.Set(artistId, dto);
                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "artist")]
        [HttpPost("schedule")]
        [ProducesResponseType(typeof(SetWorkingHoursScheduleDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SetWorkingHoursScheduleDTO>> SetSchedule(
            [FromBody] SetWorkingHoursScheduleDTO? dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                await _workingHoursService.SetSchedule(artistId, dto);
                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "artist")]
        [HttpPatch("range")]
        [ProducesResponseType(typeof(List<WorkingHoursDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<WorkingHoursDTO>>> GetInRange(
            [FromBody] WorkingHoursRangeRequestDTO? dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                var result = await _workingHoursService.GetInRange(artistId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool TryGetArtistIdFromSub(out int artistId)
        {
            artistId = 0;
            var sub = User.FindFirst("sub")?.Value;
            return sub != null && int.TryParse(sub, out artistId);
        }
    }
}
