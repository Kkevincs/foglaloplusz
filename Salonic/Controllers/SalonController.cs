using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.BLL.DTOs.SalonDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/salon")]
    [ApiController]
    public class SalonController : ControllerBase
    {
        private readonly ISalonService _salonService;
        private readonly IArtistService _artistService;

        public SalonController(ISalonService salonService, IArtistService artistService)
        {
            _salonService = salonService;
            _artistService = artistService;
        }

        /// <summary>All salons owned by the logged-in owner (JWT sub).</summary>
        [Authorize(Roles = "owner")]
        [HttpGet("my")]
        public async Task<ActionResult<List<SalonWithIdDTO>>> GetMySalons()
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            return await _salonService.GetAllForOwner(ownerId);
        }

        /// <summary>Artists in a salon when the salon belongs to the logged-in owner (JWT sub).</summary>
        [Authorize(Roles = "owner")]
        [HttpGet("{salonId:int}/artists")]
        public async Task<ActionResult<List<ArtistWithIdDTO>>> GetArtistsForSalon(int salonId)
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            var artists = await _artistService.GetBySalonIdForOwner(ownerId, salonId);
            return artists == null ? NotFound() : Ok(artists);
        }

        [Authorize(Roles = "owner")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SalonWithIdDTO>> GetById(int id)
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            var dto = await _salonService.GetByIdForOwner(ownerId, id);
            return dto == null ? NotFound() : Ok(dto);
        }

        [Authorize(Roles = "owner")]
        [HttpGet]
        public async Task<ActionResult<List<SalonWithIdDTO>>> GetAll()
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            return await _salonService.GetAllForOwner(ownerId);
        }

        [Authorize(Roles = "owner")]
        [HttpPost]
        public async Task<ActionResult<SalonWithIdDTO>> Create(SalonWithoutIdDTO salon)
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            try
            {
                var created = await _salonService.CreateForOwner(ownerId, salon);
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "owner")]
        [HttpPut]
        public async Task<ActionResult> Update(SalonWithIdDTO salon)
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            try
            {
                await _salonService.UpdateForOwner(ownerId, salon);
                return Ok();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "owner")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteById(int id)
        {
            if (!TryGetOwnerIdFromSub(out var ownerId))
                return Unauthorized();

            try
            {
                await _salonService.DeleteForOwner(ownerId, id);
                return Ok();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        private bool TryGetOwnerIdFromSub(out int ownerId)
        {
            ownerId = 0;
            var sub = User.FindFirst("sub")?.Value;
            return sub != null && int.TryParse(sub, out ownerId);
        }
    }
}
