using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    /// <summary>Salon services: public list; artists manage their own services under a salon.</summary>
    [Route("api/salon/{salonId:int}/services")]
    [ApiController]
    public class SalonServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public SalonServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        /// <summary>All services in a salon. No auth.</summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ServiceWithIdDTO>>> List(int salonId)
        {
            try
            {
                return await _serviceService.ListForSalonAsync(salonId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "artist")]
        [HttpGet("{serviceId:int}")]
        public async Task<ActionResult<ServiceWithIdDTO>> GetById(int salonId, int serviceId)
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            var dto = await _serviceService.GetByIdForArtistAsync(artistId, salonId, serviceId);
            return dto == null ? NotFound() : Ok(dto);
        }

        [Authorize(Roles = "artist")]
        [HttpPost]
        public async Task<ActionResult<ServiceWithIdDTO>> Create(int salonId, ServiceWithoutIdDTO dto)
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                var created = await _serviceService.CreateForArtistAsync(artistId, salonId, dto);
                return Ok(created);
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

        [Authorize(Roles = "artist")]
        [HttpPut]
        public async Task<ActionResult> Update(int salonId, ServiceWithIdDTO dto)
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                await _serviceService.UpdateForArtistAsync(artistId, salonId, dto);
                return Ok();
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

        [Authorize(Roles = "artist")]
        [HttpDelete("{serviceId:int}")]
        public async Task<ActionResult> Delete(int salonId, int serviceId)
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                await _serviceService.DeleteForArtistAsync(artistId, salonId, serviceId);
                return Ok();
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

        private bool TryGetArtistIdFromSub(out int artistId)
        {
            artistId = 0;
            var sub = User.FindFirst("sub")?.Value;
            return sub != null && int.TryParse(sub, out artistId);
        }
    }
}
