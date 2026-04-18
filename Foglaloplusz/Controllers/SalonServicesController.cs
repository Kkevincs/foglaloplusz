using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.ServiceDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.presentation.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/salon/{salonId:int}/services")]
[ApiController]
public class SalonServicesController : ApiControllerBase
{
    private readonly IServiceService _serviceService;

    public SalonServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [AllowAnonymous]
    [HttpGet("for-artist/{artistId:int}")]
    [SwaggerOperation(Summary = "Szolgáltatások szakemberhez", Description = "Nyilvános: egy szakember által kínált szolgáltatások a szalonban.")]
    [ProducesResponseType(typeof(List<ServiceWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ServiceWithIdDTO>>> ListForArtist(int salonId, int artistId) =>
        Ok(await _serviceService.ListOfferedForArtistInSalonAsync(salonId, artistId));

    [AllowAnonymous]
    [HttpGet]
    [SwaggerOperation(Summary = "Szalon szolgáltatásai", Description = "Nyilvános lista a szalon kínálatáról.")]
    [ProducesResponseType(typeof(List<ServiceWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ServiceWithIdDTO>>> List(int salonId) =>
        Ok(await _serviceService.ListForSalonAsync(salonId));

    [Authorize(Roles = "owner,artist")]
    [HttpGet("catalog")]
    [SwaggerOperation(Summary = "Teljes szalon katalógus", Description = "Tulajdonos vagy adott szalon szakembere: minden szolgáltatássor.")]
    [ProducesResponseType(typeof(List<ServiceWithIdDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ServiceWithIdDTO>>> ListCatalog(int salonId)
    {
        if (User.IsInRole("owner"))
        {
            if (!TryGetUserIdFromSub(out var ownerId))
                return Unauthorized();
            var list = await _serviceService.ListCatalogForSalonOwnerAsync(ownerId, salonId);
            return Ok(list);
        }

        if (User.IsInRole("artist"))
        {
            if (!TryGetUserIdFromSub(out var artistId))
                return Unauthorized();
            var list = await _serviceService.ListCatalogForSalonArtistAsync(artistId, salonId);
            return Ok(list);
        }

        return Forbid();
    }

    [Authorize(Roles = "owner")]
    [HttpPost("import")]
    [SwaggerOperation(Summary = "Tulaj CSV import", Description = "Szalon katalógus tömeges feltöltése.")]
    [ProducesResponseType(typeof(ServiceCsvImportResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceCsvImportResponseDTO>> ImportCatalogFromCsv(
        int salonId,
        [FromBody] ServiceCsvImportRequestDTO? body)
    {
        if (body == null || string.IsNullOrWhiteSpace(body.CsvText))
            return BadRequest("CSV szöveg kötelező.");
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var result = await _serviceService.ImportSalonOwnerServicesFromCsvAsync(ownerId, salonId, body.CsvText);
        return Ok(result);
    }

    [Authorize(Roles = "owner")]
    [HttpPost]
    [SwaggerOperation(Summary = "Új katalógus-szolgáltatás (tulaj)", Description = "Szalon szintű szolgáltatás létrehozása.")]
    [ProducesResponseType(typeof(ServiceWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceWithIdDTO>> CreateCatalogService(int salonId, [FromBody] ServiceWithoutIdDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var created = await _serviceService.CreateForSalonOwnerAsync(ownerId, salonId, dto);
        return Ok(created);
    }

    [Authorize(Roles = "owner")]
    [HttpPut("{serviceId:int}")]
    [SwaggerOperation(Summary = "Katalógus-szolgáltatás szerkesztése", Description = "Tulajdonos módosítja a szalon katalógusát.")]
    [ProducesResponseType(typeof(ServiceWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ServiceWithIdDTO>> UpdateCatalogService(
        int salonId,
        int serviceId,
        [FromBody] ServiceWithoutIdDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var updated = await _serviceService.UpdateForSalonOwnerAsync(ownerId, salonId, serviceId, dto);
        return Ok(updated);
    }

    [Authorize(Roles = "owner")]
    [HttpDelete("{serviceId:int}")]
    [SwaggerOperation(Summary = "Katalógus-szolgáltatás törlése", Description = "Tulajdonos törli a szolgáltatást.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCatalogService(int salonId, int serviceId)
    {
        if (!TryGetUserIdFromSub(out var ownerId))
            return Unauthorized();
        var ok = await _serviceService.DeleteForSalonOwnerAsync(ownerId, salonId, serviceId);
        if (!ok)
            return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "artist")]
    [HttpGet("{serviceId:int}")]
    [SwaggerOperation(Summary = "Szolgáltatás részletei (szakember)", Description = "Egy szolgáltatás megtekintése, ha a szakemberhez kötve van.")]
    [ProducesResponseType(typeof(ServiceWithIdDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ServiceWithIdDTO>> GetById(int salonId, int serviceId)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var dto = await _serviceService.GetByIdForArtistAsync(artistId, salonId, serviceId);
        if (dto == null)
            return Forbid();
        return Ok(dto);
    }
}
