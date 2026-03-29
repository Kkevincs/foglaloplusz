using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/Artist")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly IArtistService _artistService;
        private readonly IServiceService _serviceService;

        public ArtistController(IArtistService artist, IServiceService serviceService)
        {
            _artistService = artist;
            _serviceService = serviceService;
        }

        [HttpGet("me")]
        [Authorize(Roles = "artist")]
        public async Task<ActionResult<ArtistWithIdDTO>> GetMe()
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            return await _artistService.GetById(artistId);
        }

        [Authorize(Roles = "artist")]
        [HttpPost("services")]
        public async Task<ActionResult<ServiceWithIdDTO>> CreateMyService(ServiceWithoutIdDTO dto)
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                var created = await _serviceService.CreateForCurrentArtistAsync(artistId, dto);
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
        [HttpPut("services")]
        public async Task<ActionResult> UpdateMyService(ServiceWithIdDTO dto)
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                await _serviceService.UpdateForCurrentArtistAsync(artistId, dto);
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
        [HttpDelete("services/{serviceId:int}")]
        public async Task<ActionResult> DeleteMyService(int serviceId)
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                await _serviceService.DeleteForCurrentArtistAsync(artistId, serviceId);
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

        [HttpGet]
        [Authorize(Roles = "admin,owner,guest,artist")]
        public async Task<ActionResult<List<ArtistWithIdDTO>>> GetAll()
        {
            return await _artistService.GetAll();
        }

        [HttpPut]
        [Authorize(Roles = "admin,owner,artist")]
        public async Task Update(ArtistWithIdDTO artist)
        {
            await _artistService.Update(artist);
        }

        [HttpDelete]
        [Authorize(Roles = "admin,owner,artist")]
        public async Task DeleteById(int id)
        {
            await _artistService.DeleteById(id);
        }

        private bool TryGetArtistIdFromSub(out int artistId)
        {
            artistId = 0;
            var sub = User.FindFirst("sub")?.Value;
            return sub != null && int.TryParse(sub, out artistId);
        }
    }
}
