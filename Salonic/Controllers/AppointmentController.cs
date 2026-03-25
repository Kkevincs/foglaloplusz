using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/appointment")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        [Authorize(Roles ="admin")]
        [HttpGet]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetAll()
        { 
            return await _appointmentService.GetAll();
        }
        [Authorize(Roles = "admin,owner")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentsWithIdDTO>> GetById(int id)
        { 
            return await _appointmentService.GetById(id);
        }



        [Authorize(Roles = "admin,owner,artist")]
        [HttpGet("artist")]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetByArtistId()
        {
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();

            int Id = Convert.ToInt32(subClaim);
            return await _appointmentService.GetByArtistId(Id);
        }

        [Authorize(Roles = "admin,owner,guest")]
        [HttpGet("guest")]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetByGuestId()
        {
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();

            int Id = Convert.ToInt32(subClaim);
            return await _appointmentService.GetByGuestId(Id);
        }

        [Authorize(Roles = "guest")]
        [HttpPost]
        public async Task<ActionResult<AppointmentsWithoutIdDTO>> Create(AppointmentsWithoutIdDTO dto)
        {
            try
            {
                return await _appointmentService.Create(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPatch("available")]
        public async Task<ActionResult<List<AvaliableSlotDTO>>> GetAvailableSlots(
            GetAvailableSlotsDTO request)
        {
            return await _appointmentService.GetAvailableSlots(
                request.ArtistId,
                request.ServiceIds.ToArray(),
                request.Date);
        }
        [Authorize(Roles ="owner,artist,guest")]
        [HttpPut]

        public async Task Update(AppointmentsWithIdDTO app) { 
            await _appointmentService.Update(app);
        }

        [HttpDelete]    
        [Authorize(Roles ="owner,artist,guest")]
        public async Task DeleteById(int id) { 
            await _appointmentService.DeleteById(id);
        }


    }
}
