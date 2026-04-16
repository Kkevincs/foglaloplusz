using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Foglaloplusz.BLL.DTOs.ArtistDTOs;
using Foglaloplusz.BLL.DTOs.GuestDTOs;
using Foglaloplusz.BLL.DTOs.OwnerDTOs;
using Foglaloplusz.BLL.Services.Interfaces;

namespace Foglaloplusz.presentation.Controllers
{
    [Route("api/register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IOwnerService _ownerService;
        private readonly IArtistService _artistService;
        private readonly IGuestService _guestService;
        private readonly IAuthenticationService _authenticationService;

        public RegisterController(
            IOwnerService ownerService,
            IArtistService artistService,
            IGuestService guestService,
            IAuthenticationService authenticationService)
        {
            _ownerService = ownerService;
            _artistService = artistService;
            _guestService = guestService;
            _authenticationService = authenticationService;
        }

        [HttpPost("guest")]
        [ProducesResponseType(typeof(GuestWithoutIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GuestWithoutIdDTO>> RegisterGuest([FromBody] GuestWithoutIdDTO? user)
        {
            if (user == null)
                return BadRequest("Request body is required.");

            if (await _authenticationService.EmailExistsAsync(user.Email))
                return BadRequest("A user with this email is already registered.");

            var created = await _guestService.CreateAsync(user);
            return Ok(created);
        }

        [HttpPost("artist")]
        [ProducesResponseType(typeof(ArtistWithoutIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ArtistWithoutIdDTO>> RegisterArtist([FromBody] ArtistWithoutIdDTO? user)
        {
            if (user == null)
                return BadRequest("Request body is required.");

            if (await _authenticationService.EmailExistsAsync(user.Email))
                return BadRequest("A user with this email is already registered.");

            var created = await _artistService.CreateAsync(user);
            return Ok(created);
        }

        [HttpPost("owner")]
        [ProducesResponseType(typeof(OwnerWithoutIdDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OwnerWithoutIdDTO>> RegisterOwner([FromBody] OwnerWithoutIdDTO? user)
        {
            if (user == null)
                return BadRequest("Request body is required.");

            if (await _authenticationService.EmailExistsAsync(user.Email))
                return BadRequest("A user with this email is already registered.");

            var created = await _ownerService.CreateAsync(user);
            return Ok(created);
        }
    }
}
