using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.presentation.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace Foglaloplusz.presentation.Controllers;

[Route("api/workinghours")]
[ApiController]
public class WorkingHoursController : ApiControllerBase
{
    private readonly IWorkingHoursService _workingHoursService;

    public WorkingHoursController(IWorkingHoursService workingHoursService)
    {
        _workingHoursService = workingHoursService;
    }

    [Authorize(Roles = "artist")]
    [HttpPost]
    [SwaggerOperation(Summary = "Munkaidő egy napra", Description = "Egy nap kezdő és záró óráinak beállítása.")]
    [ProducesResponseType(typeof(WorkingHoursDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WorkingHoursDTO>> SetWorkingHours([FromBody] WorkingHoursDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        await _workingHoursService.Set(artistId, dto);
        return Ok(dto);
    }

    [Authorize(Roles = "artist")]
    [HttpPost("schedule")]
    [SwaggerOperation(Summary = "Heti ütemterv", Description = "Több nap munkaidejének megadása egyszerre.")]
    [ProducesResponseType(typeof(SetWorkingHoursScheduleDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SetWorkingHoursScheduleDTO>> SetSchedule([FromBody] SetWorkingHoursScheduleDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        await _workingHoursService.SetSchedule(artistId, dto);
        return Ok(dto);
    }

    [Authorize(Roles = "artist")]
    [HttpPatch("range")]
    [SwaggerOperation(Summary = "Munkaidő lekérése tartományban", Description = "Napok listája a megadott intervallumra.")]
    [ProducesResponseType(typeof(List<WorkingHoursDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<WorkingHoursDTO>>> GetInRange([FromBody] WorkingHoursRangeRequestDTO? dto)
    {
        if (!TryGetUserIdFromSub(out var artistId))
            return Unauthorized();
        var result = await _workingHoursService.GetInRange(artistId, dto);
        return Ok(result);
    }
}
