using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Services.Interfaces;

namespace Foglaloplusz.presentation.Controllers
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

        [AllowAnonymous]
        [HttpPost("names")]
        [ProducesResponseType(typeof(BookingNamesResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookingNamesResponseDTO>> GetBookingNames(
            [FromBody] BookingNamesRequestDTO? request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            try
            {
                var result = await _serviceService.GetBookingDisplayNamesAsync(request);
                return Ok(result);
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
                return BadRequest(ex.Message);
            }
        }
    }
}
