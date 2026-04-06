using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/guest")]
    [ApiController]
    public class GuestController : ControllerBase
    {
        private readonly IGuestService _guestService;

        public GuestController(IGuestService guest)
        {
            _guestService = guest;
        }

        [Authorize(Roles = "owner,guest")]
        [HttpGet("me")]
        [ProducesResponseType(typeof(GuestWithIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GuestWithIdDTO>> GetById()
        {
            if (!TryGetIntClaim("sub", out var id))
                return Unauthorized();

            var guest = await _guestService.GetByIdAsync(id);
            if (guest == null)
            {
                if (User.IsInRole("guest"))
                    return BadRequest("Guest profile was not found.");
                return Forbid();
            }

            return Ok(guest);
        }

        private bool TryGetIntClaim(string type, out int value)
        {
            value = 0;
            var raw = User.FindFirst(type)?.Value;
            return raw != null && int.TryParse(raw, out value);
        }
    }
}
