using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(typeof(ArtistWithIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ArtistWithIdDTO>> GetMe()
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            var dto = await _artistService.GetByIdAsync(artistId);
            if (dto == null)
                return Forbid();

            return Ok(dto);
        }

        [Authorize(Roles = "artist")]
        [HttpGet("services")]
        [ProducesResponseType(typeof(List<ServiceWithIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<ServiceWithIdDTO>>> GetMyServices()
        {
            if (!TryGetArtistIdFromSub(out var artistId))
                return Unauthorized();

            try
            {
                var list = await _serviceService.ListForCurrentArtistAsync(artistId);
                return Ok(list);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "artist")]
        [HttpPost("services")]
        [ProducesResponseType(typeof(ServiceWithIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ServiceWithIdDTO>> CreateMyService([FromBody] ServiceWithoutIdDTO? dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

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

        [HttpGet]
        [Authorize(Roles = "owner,guest,artist")]
        [ProducesResponseType(typeof(List<ArtistWithIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<ArtistWithIdDTO>>> GetAll()
        {
            var list = await _artistService.ListAllAsync();
            return Ok(list);
        }

        private bool TryGetArtistIdFromSub(out int artistId)
        {
            artistId = 0;
            var sub = User.FindFirst("sub")?.Value;
            return sub != null && int.TryParse(sub, out artistId);
        }
    }
}
