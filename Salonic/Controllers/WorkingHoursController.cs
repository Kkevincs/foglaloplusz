using Microsoft.AspNetCore.Authorization;
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
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();
            int Id = Convert.ToInt32(subClaim);
            try
            {
                await _workingHoursService.Set(Id, dto);
                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "artist,admin")]
        [HttpPost("schedule")]
        public async Task<ActionResult> SetSchedule(SetWorkingHoursScheduleDTO dto)
        {
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();
            int Id = Convert.ToInt32(subClaim);
            try
            {
                await _workingHoursService.SetSchedule(Id, dto);
                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest();
            }
            
        }

       
    }
}
