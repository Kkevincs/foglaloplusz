using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.DTOs.OwnerDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
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


        [Authorize(Roles ="admin,owner")]
        [HttpGet("me")]
        public async Task<ActionResult<OwnerWithIdDTO>> GetById(int id)
        {
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();

            int Id = Convert.ToInt32(subClaim);
            return await _ownerService.GetById(Id);
        }
        [Authorize(Roles ="admin")]
        [HttpGet]
        public async Task<ActionResult<List<OwnerWithIdDTO>>> GetAll()
        {
            return await _ownerService.GetAll();
        }


        [Authorize(Roles = "admin,owner")]
        [HttpPut]
        public async Task Update(OwnerWithIdDTO owner)
        {
            await _ownerService.Update(owner);
        }
        [Authorize(Roles = "admin,owner")]
        [HttpDelete]
        public async Task DeleteById(int id)
        {
            await _ownerService.DeleteById(id);
        }
    }
}
