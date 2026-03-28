using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
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
        [Authorize(Roles ="admin,owner,guest")]
        [HttpGet("me")]
        public async Task<ActionResult<GuestWithIdDTO>> GetById()
        {
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();

            int Id = Convert.ToInt32(subClaim);
            return await _guestService.GetById(Id);
        }
        [Authorize(Roles ="admin,owner")]
        [HttpGet]

        public async Task<ActionResult<List<GuestWithIdDTO>>> GetAll()
        {
            return await _guestService.GetAll();
        }

        [Authorize(Roles ="admin,owner,guest")]
        [HttpPut]

        public async Task Update(GuestWithIdDTO guest)
        {
            await _guestService.Update(guest);
        }
        [Authorize(Roles = "admin,owner")]
        [HttpDelete]

        public async Task DeleteById(int id)
        {
            await _guestService.DeleteById(id);
        }
    }
}
