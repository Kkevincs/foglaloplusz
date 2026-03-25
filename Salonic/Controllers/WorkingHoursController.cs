using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
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

        [Authorize(Roles = "artist,admin")]
        [HttpPost]
        public async Task<ActionResult> SetWorkingHours(WorkingHoursDTO dto)
        {
            await _workingHoursService.Set(dto);
            return Ok();
        }
    }
}
