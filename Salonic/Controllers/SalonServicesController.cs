using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/salon/{salonId:int}/services")]
    [ApiController]
    public class SalonServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public SalonServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(List<ServiceWithIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ServiceWithIdDTO>>> List(int salonId)
        {
            try
            {
                var list = await _serviceService.ListForSalonAsync(salonId);
                return Ok(list);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "artist")]
        [HttpGet("{serviceId:int}")]
        [ProducesResponseType(typeof(ServiceWithIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ServiceWithIdDTO>> GetById(int salonId, int serviceId)
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            var dto = await _serviceService.GetByIdForArtistAsync(artistId, salonId, serviceId);
            if (dto == null)
                return Forbid();

            return Ok(dto);
        }

        private bool TryGetArtistIdFromSub(out int artistId)
        {
            artistId = 0;
            var sub = User.FindFirst("sub")?.Value;
            return sub != null && int.TryParse(sub, out artistId);
        }
    }
}
