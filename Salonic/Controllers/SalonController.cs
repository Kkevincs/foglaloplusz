using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.DTOs.ArtistDTOs;
using Foglaloplusz.BLL.DTOs.SalonDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.BLL.Utilities;
using Foglaloplusz.FileStorage;
using Foglaloplusz.presentation.Infrastructure;
using Foglaloplusz.presentation.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/salon")]
[ApiController]
public class SalonController : ApiControllerBase
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
    [HttpGet("public")]
    [SwaggerOperation(Summary = "Nyilvános szalonlista", Description = "Minden szalon alapadatai (portál / kezdőlap).")]
    [ProducesResponseType(typeof(List<SalonWithIdDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SalonWithIdDTO>>> GetPublicSalons()
    {
        var list = await _salonService.ListPublicAsync();
        return Ok(list);
    }

    [AllowAnonymous]
    [HttpGet("{salonId:int}/exists")]
    [SwaggerOperation(Summary = "Szalon létezik-e", Description = "204 ha van ilyen azonosító, egyébként 404.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SalonExists(int salonId)
    {
        if (!await _salonService.ExistsAsync(salonId))
            return NotFound();
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("directory")]
    [SwaggerOperation(Summary = "Szalon könyvtár", Description = "Kártyák a böngésző / portál nézethez.")]
    [ProducesResponseType(typeof(List<SalonDirectoryCardDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SalonDirectoryCardDTO>>> GetPublicDirectory()
    {
        var list = await _salonService.ListPublicDirectoryAsync();
        return Ok(list);
    }

    [AllowAnonymous]
    [HttpGet("{salonId:int}/public-profile")]
    [SwaggerOperation(Summary = "Szalon nyilvános profil", Description = "Leírás, téma, elérhetőség a vendégoldalhoz.")]
    [ProducesResponseType(typeof(SalonWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalonWithIdDTO>> GetPublicProfile(int salonId)
    {
        var profile = await _salonService.GetPublicProfileAsync(salonId);
        if (profile == null)
            return NotFound();
        return Ok(profile);
    }

    [AllowAnonymous]
    [HttpGet("{salonId:int}/public-artists")]
    [SwaggerOperation(Summary = "Nyilvános szakemberlista", Description = "Opcionális szolgáltatás-szűrő: serviceIds query (vesszővel).")]
    [ProducesResponseType(typeof(List<ArtistWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ArtistWithIdDTO>>> GetPublicArtistsForSalon(
        int salonId,
        [FromQuery(Name = "serviceIds")] string? serviceIdsCsv = null)
    {
        if (!await _salonService.ExistsAsync(salonId))
            return NotFound();

        var filter = QueryParameterParsers.ParseOptionalPositiveIntList(serviceIdsCsv);
        var list = await _artistService.ListBySalonForPublicAsync(salonId, filter);
        return Ok(list);
    }

    [Authorize(Roles = "owner")]
    [HttpGet("my")]
    [SwaggerOperation(Summary = "Saját szalonjaim", Description = "A bejelentkezett tulajdonos szalonjai.")]
    [ProducesResponseType(typeof(List<SalonWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SalonWithIdDTO>>> GetMySalons()
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var list = await _salonService.ListForOwnerAsync(ownerId);
        return Ok(list);
    }

    [Authorize(Roles = "owner")]
    [HttpGet("{salonId:int}/artists")]
    [SwaggerOperation(Summary = "Szakemberek (tulaj)", Description = "A szalon dolgozói, ha a tulajdonoshoz tartozik a szalon.")]
    [ProducesResponseType(typeof(List<ArtistWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ArtistWithIdDTO>>> GetArtistsForSalon(int salonId)
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var artists = await _artistService.ListBySalonForOwnerAsync(ownerId, salonId);
        return Ok(artists);
    }

    [Authorize(Roles = "owner")]
    [HttpGet("{salonId:int}/artists/{artistId:int}/schedule")]
    [SwaggerOperation(Summary = "Heti naptár (tulaj)", Description = "Szakember foglaltsági nézete egy hétre.")]
    [ProducesResponseType(typeof(ArtistWeekScheduleDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ArtistWeekScheduleDTO>> GetArtistWeekSchedule(
        int salonId,
        int artistId,
        [FromQuery] DateTime? weekStart,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserIdFromSub(out var ownerId))
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
    [SwaggerOperation(Summary = "Szalon törlése", Description = "Csak a tulajdonos törölheti; kapcsolódó adatok is törlődnek.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int salonId, CancellationToken cancellationToken)
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        await _salonService.DeleteForOwnerAsync(ownerId, salonId, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "owner")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Új szalon", Description = "Multipart: név, e-mail, téma, leírás, opcionális logó.")]
    [ProducesResponseType(typeof(SalonWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SalonWithIdDTO>> Create(
        [FromForm] SalonCreateForm? form,
        CancellationToken cancellationToken)
    {
        if (form == null)
            return BadRequest("Használj multipart/form-data mezőket: name, email, theme, description, opcionális logo fájl.");

        if (!TryGetUserIdFromSub(out var ownerId))
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
            Location = string.IsNullOrWhiteSpace(form.Location) ? null : form.Location.Trim(),
            Logo = logoPath ?? string.Empty
        };

        var created = await _salonService.CreateForOwnerAsync(ownerId, salon);
        return Ok(created);
    }

    [Authorize(Roles = "owner")]
    [HttpPut("{salonId:int}")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Szalon szerkesztése", Description = "Ugyanaz a multipart séma, mint a létrehozásnál.")]
    [ProducesResponseType(typeof(SalonWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalonWithIdDTO>> Update(
        int salonId,
        [FromForm] SalonCreateForm? form,
        CancellationToken cancellationToken)
    {
        if (form == null)
            return BadRequest("Használj multipart/form-data mezőket: name, email, theme, description, opcionális location és logo.");

        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string? newLogoPath = null;
        if (form.Logo != null && form.Logo.Length > 0)
        {
            try
            {
                newLogoPath = await _salonImageStorage.SaveSalonLogoAsync(form.Logo, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        var dto = new SalonWithoutIdDTO
        {
            Name = form.Name.Trim(),
            Email = form.Email.Trim(),
            Theme = form.Theme?.Trim() ?? string.Empty,
            Description = form.Description?.Trim() ?? string.Empty,
            Location = string.IsNullOrWhiteSpace(form.Location) ? null : form.Location.Trim(),
            Logo = string.Empty,
        };

        var updated = await _salonService.UpdateForOwnerAsync(ownerId, salonId, dto, newLogoPath, cancellationToken);
        return Ok(updated);
    }

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
