using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.Services;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/appointment")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAvailabilityService _availabilityService;

        public AppointmentController(IAppointmentService appointmentService, IAvailabilityService availabilityService)
        {
            _appointmentService = appointmentService;
            _availabilityService = availabilityService;

        }

        /// <summary>Appointments for the logged-in artist (from JWT sub claim).</summary>
        [Authorize(Roles = "artist")]
        [HttpGet("my")]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetMyAppointments()
        {
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();

            int Id = Convert.ToInt32(subClaim);

            return await _appointmentService.GetByArtistId(Id);
        }

        /// <summary>All appointments for a salon when the salon belongs to the logged-in owner (JWT sub).</summary>
        [Authorize(Roles = "owner")]
        [HttpGet("salon/{salonId:int}")]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetBySalonForOwner(int salonId)
        {
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();

            int Id = Convert.ToInt32(subClaim);

            try
            {
                return await _appointmentService.GetBySalonIdForOwnerAsync(Id, salonId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
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
            return await _appointmentService.Create(dto);
        }

        [AllowAnonymous]
        [HttpPatch("available")]
        public async Task<ActionResult<List<AvaliableSlotDTO>>> GetAvailableSlots(
            GetAvailableSlotsDTO request)
        {
            return await _appointmentService.GetAvailableSlots(
                request.ArtistId,
                request.ServiceIds.ToArray(),
                request.Date); //TODO Modify them to be a dto
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

        private bool TryGetArtistIdFromSub(out int artistId)
        {
            artistId = 0;
            var sub = User.FindFirst("sub")?.Value;
            return sub != null && int.TryParse(sub, out artistId);
        }

        
    }
}
