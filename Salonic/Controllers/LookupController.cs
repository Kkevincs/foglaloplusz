using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/lookup")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public LookupController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        /// <summary>Resolve artist, guest, and service display names for a booking context. No auth.</summary>
        [AllowAnonymous]
        [HttpPost("names")]
        public async Task<ActionResult<BookingNamesResponseDTO>> GetBookingNames([FromBody] BookingNamesRequestDTO request)
        {
            try
            {
                return await _serviceService.GetBookingDisplayNamesAsync(request);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
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
    }
}
