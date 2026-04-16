using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.OwnerDTOs;
using Foglaloplusz.BLL.Services.Interfaces;

namespace Foglaloplusz.presentation.Controllers
{
    [Route("api/owner")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IOwnerService _ownerService;

        public OwnerController(IOwnerService owner)
        {
            _ownerService = owner;
        }

        [Authorize(Roles = "owner")]
        [HttpGet("me")]
        [ProducesResponseType(typeof(OwnerWithIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OwnerWithIdDTO>> GetMe()
        {
            if (!TryGetIntClaim("sub", out var ownerId))
                return Unauthorized();

            var owner = await _ownerService.GetByIdAsync(ownerId);
            if (owner == null)
                return Forbid();

            return Ok(owner);
        }

        private bool TryGetIntClaim(string type, out int value)
        {
            value = 0;
            var raw = User.FindFirst(type)?.Value;
            return raw != null && int.TryParse(raw, out value);
        }
    }
}
