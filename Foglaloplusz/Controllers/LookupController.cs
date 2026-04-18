using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/lookup")]
[AllowAnonymous]
[ApiController]
public class LookupController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public LookupController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpPost("names")]
    [SwaggerOperation(Summary = "Foglalás megjelenítő nevek", Description = "Vendég, szakember és szolgáltatásnevek lekérése azonosítók alapján.")]
    [ProducesResponseType(typeof(BookingNamesResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookingNamesResponseDTO>> GetBookingNames([FromBody] BookingNamesRequestDTO? request) =>
        Ok(await _serviceService.GetBookingDisplayNamesAsync(request));
}
