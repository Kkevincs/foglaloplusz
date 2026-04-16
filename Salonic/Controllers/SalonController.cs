using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.DTOs.ArtistDTOs;
using Foglaloplusz.BLL.DTOs.SalonDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.FileStorage;
using Foglaloplusz.presentation.Models;

namespace Foglaloplusz.presentation.Controllers
{
    [Route("api/salon")]
    [ApiController]
    public class SalonController : ControllerBase
    {
        private readonly ISalonService _salonService;
        private readonly IArtistService _artistService;
        private readonly IArtistScheduleService _artistScheduleService;
        private readonly ISalonImageStorage _salonImageStorage;

        public SalonController(
            ISalonService salonService,
            IArtistService artistService,
            IArtistScheduleService artistScheduleService,
            ISalonImageStorage salonImageStorage)
        {
            _salonService = salonService;
            _artistService = artistService;
            _artistScheduleService = artistScheduleService;
            _salonImageStorage = salonImageStorage;
        }

        [AllowAnonymous]
        [HttpGet("{salonId:int}/exists")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SalonExists(int salonId)
        {
            if (!await _salonService.ExistsAsync(salonId))
                return NotFound();
            return NoContent();
        }

        /// <summary>Public list of all salons for the landing portal (browse).</summary>
        [AllowAnonymous]
        [HttpGet("directory")]
        [ProducesResponseType(typeof(List<SalonDirectoryCardDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SalonDirectoryCardDTO>>> GetPublicDirectory()
        {
            var list = await _salonService.ListPublicDirectoryAsync();
            return Ok(list);
        }

        [Authorize(Roles = "owner")]
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<SalonWithIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<SalonWithIdDTO>>> GetMySalons()
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            var list = await _salonService.ListForOwnerAsync(ownerId);
            return Ok(list);
        }

        [Authorize(Roles = "owner")]
        [HttpGet("{salonId:int}/artists")]
        [ProducesResponseType(typeof(List<ArtistWithIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<ArtistWithIdDTO>>> GetArtistsForSalon(int salonId)
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            var artists = await _artistService.ListBySalonForOwnerAsync(ownerId, salonId);
            if (artists == null)
                return Forbid();

            return Ok(artists);
        }

        [Authorize(Roles = "owner")]
        [HttpGet("{salonId:int}/artists/{artistId:int}/schedule")]
        [ProducesResponseType(typeof(ArtistWeekScheduleDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ArtistWeekScheduleDTO>> GetArtistWeekSchedule(
            int salonId,
            int artistId,
            [FromQuery] DateTime? weekStart,
            CancellationToken cancellationToken)
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            var monday = weekStart is { } ws && ws != default
                ? GetMondayOfWeek(ws.Date)
                : GetMondayOfWeek(DateTime.Today);

            var schedule = await _artistScheduleService.GetWeekForOwnerAsync(
                ownerId,
                salonId,
                artistId,
                monday,
                cancellationToken);

            if (schedule == null)
                return Forbid();

            return Ok(schedule);
        }

        [Authorize(Roles = "owner")]
        [HttpDelete("{salonId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int salonId, CancellationToken cancellationToken)
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            try
            {
                await _salonService.DeleteForOwnerAsync(ownerId, salonId, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "owner")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(SalonWithIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SalonWithIdDTO>> Create(
            [FromForm] SalonCreateForm? form,
            CancellationToken cancellationToken)
        {
            if (form == null)
                return BadRequest("Use multipart/form-data with fields: name, email, theme, description, and optional logo (file).");

            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? logoPath;
            try
            {
                logoPath = await _salonImageStorage.SaveSalonLogoAsync(form.Logo, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            var salon = new SalonWithoutIdDTO
            {
                Name = form.Name.Trim(),
                Email = form.Email.Trim(),
                Theme = form.Theme?.Trim() ?? string.Empty,
                Description = form.Description?.Trim() ?? string.Empty,
                Logo = logoPath ?? string.Empty
            };

            try
            {
                var created = await _salonService.CreateForOwnerAsync(ownerId, salon);
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool TryGetOwnerIdFromSub(out int ownerId)
        {
            ownerId = 0;
            var sub = User.FindFirst("sub")?.Value;
            return sub != null && int.TryParse(sub, out ownerId);
        }

        /// <summary>ISO-style week: Monday is the first day.</summary>
        private static DateTime GetMondayOfWeek(DateTime date)
        {
            var d = date.Date;
            return d.DayOfWeek switch
            {
                DayOfWeek.Tuesday => d.AddDays(-1),
                DayOfWeek.Wednesday => d.AddDays(-2),
                DayOfWeek.Thursday => d.AddDays(-3),
                DayOfWeek.Friday => d.AddDays(-4),
                DayOfWeek.Saturday => d.AddDays(-5),
                DayOfWeek.Sunday => d.AddDays(-6),
                _ => d
            };
        }
    }
}
