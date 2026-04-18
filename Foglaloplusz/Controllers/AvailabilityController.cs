using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/availability")]
[AllowAnonymous]
[ApiController]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpPost("month")]
    [SwaggerOperation(Summary = "Havi elérhetőség", Description = "Napokra bontva jelzi, van-e szabad időpont (nyilvános).")]
    [ProducesResponseType(typeof(List<DayAvailabilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DayAvailabilityDto>>> GetMonthAvailability(
        [FromBody] MonthAvailabilityRequestDto? request) =>
        Ok(await _availabilityService.GetMonthAvailability(request));
}
