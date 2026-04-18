using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.ArtistDTOs;
using Foglaloplusz.BLL.DTOs.ServiceDTOs;
using Foglaloplusz.BLL.DTOs.UserDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.presentation.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/Artist")]
[ApiController]
public class ArtistController : ApiControllerBase
{
    private readonly IArtistService _artistService;
    private readonly IServiceService _serviceService;

    public ArtistController(IArtistService artist, IServiceService serviceService)
    {
        _artistService = artist;
        _serviceService = serviceService;
    }

    [HttpGet("me")]
    [Authorize(Roles = "artist")]
    [SwaggerOperation(Summary = "Szakember profil", Description = "A bejelentkezett szakember adatai.")]
    [ProducesResponseType(typeof(ArtistWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtistWithIdDTO>> GetMe()
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var dto = await _artistService.GetMeOrThrowAsync(artistId);
        return Ok(dto);
    }

    [Authorize(Roles = "artist")]
    [HttpPut("me")]
    [SwaggerOperation(Summary = "Szakember profil módosítása", Description = "Név, e-mail, leírás, opcionális jelszócsere.")]
    [ProducesResponseType(typeof(ArtistWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtistWithIdDTO>> UpdateMe([FromBody] UserProfileUpdateDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var updated = await _artistService.UpdateProfileAsync(artistId, dto!);
        return Ok(updated);
    }

    [Authorize(Roles = "artist")]
    [HttpGet("services")]
    [SwaggerOperation(Summary = "Saját szolgáltatások", Description = "A szakember által kínált szolgáltatások (offeringekkel).")]
    [ProducesResponseType(typeof(List<ServiceWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ServiceWithIdDTO>>> GetMyServices()
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var list = await _serviceService.ListForCurrentArtistAsync(artistId);
        return Ok(list);
    }

    [Authorize(Roles = "artist")]
    [HttpPost("services")]
    [SwaggerOperation(Summary = "Új saját szolgáltatás", Description = "Szolgáltatás létrehozása a szakember szalonjában.")]
    [ProducesResponseType(typeof(ServiceWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceWithIdDTO>> CreateMyService([FromBody] ServiceWithoutIdDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var created = await _serviceService.CreateForCurrentArtistAsync(artistId, dto);
        return Ok(created);
    }

    [Authorize(Roles = "artist")]
    [HttpPut("services/{serviceId:int}")]
    [SwaggerOperation(Summary = "Saját szolgáltatás szerkesztése", Description = "Csak a szakember által létrehozott szolgáltatás módosítható.")]
    [ProducesResponseType(typeof(ServiceWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceWithIdDTO>> UpdateMyService(int serviceId, [FromBody] ServiceWithoutIdDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var updated = await _serviceService.UpdateForCurrentArtistAsync(artistId, serviceId, dto);
        return Ok(updated);
    }

    [Authorize(Roles = "artist")]
    [HttpPost("services/link")]
    [SwaggerOperation(Summary = "Katalógus-szolgáltatás felvétele", Description = "Tulaj által létrehozott szalonkatalógus-szolgáltatás hozzárendelése.")]
    [ProducesResponseType(typeof(ServiceWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceWithIdDTO>> LinkSalonCatalogService([FromBody] ServiceLinkRequestDTO? body)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        if (body == null || body.ServiceId <= 0)
            return BadRequest("serviceId kötelező.");
        var created = await _serviceService.LinkSalonServiceForArtistAsync(artistId, body.ServiceId);
        return Ok(created);
    }

    [Authorize(Roles = "artist")]
    [HttpDelete("services/offerings/{offeringId:int}")]
    [SwaggerOperation(Summary = "Importált offering törlése", Description = "Csak importált (nem saját) offering távolítható el.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkImportedOffering(int offeringId)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var ok = await _serviceService.UnlinkOfferingForArtistAsync(artistId, offeringId);
        if (!ok)
            return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "artist")]
    [HttpPost("services/import")]
    [SwaggerOperation(Summary = "Szolgáltatások CSV import", Description = "Tömeges feltöltés pontosvesszővel tagolt CSV-ből.")]
    [ProducesResponseType(typeof(ServiceCsvImportResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceCsvImportResponseDTO>> ImportServicesFromCsv([FromBody] ServiceCsvImportRequestDTO? body)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        if (body == null || string.IsNullOrWhiteSpace(body.CsvText))
            return BadRequest("CSV szöveg kötelező.");
        var result = await _serviceService.ImportServicesFromCsvAsync(artistId, body.CsvText);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "owner,guest,artist")]
    [SwaggerOperation(Summary = "Összes szakember (admin jelleg)", Description = "Listázás tulajdonos, vendég vagy szakember szerepkörrel.")]
    [ProducesResponseType(typeof(List<ArtistWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ArtistWithIdDTO>>> GetAll()
    {
        var list = await _artistService.ListAllAsync();
        return Ok(list);
    }
}
