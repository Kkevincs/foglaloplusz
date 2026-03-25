using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Salonic.BLL.DTOs.AdminDTOs;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.DTOs.OwnerDTOs;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IOwnerService _ownerService;
        private readonly IArtistService _artistService;
        private readonly IGuestService _guestService;
        private readonly IAdminService _adminService;

        public RegisterController(IOwnerService ownerService, IArtistService artistService, IGuestService guestService, IAdminService adminService)
        {
            _ownerService = ownerService;
            _artistService = artistService;
            _guestService = guestService;
            _adminService = adminService;
        }

        [HttpPost("guest")]
        public async Task<GuestWithoutIdDTO> RegisterGuest(GuestWithoutIdDTO user) {
            return await _guestService.Create(user);
            
        }
        [HttpPost("artist")]
        public async Task<ArtistWithoutIdDTO> RegisterArtist(ArtistWithoutIdDTO user)
        {
            return await _artistService.Create(user);

        }
        [HttpPost("owner")]
        public async Task<OwnerWithoutIdDTO> RegisterOwner(OwnerWithoutIdDTO user)
        {
            return await _ownerService.Create(user);

        }
        [HttpPost("admin")]
        public async Task<AdminWithoutIdDTO> RegisterAdmin(AdminWithoutIdDTO user)
        {
            return await _adminService.Create(user);

        }
    }
}
