using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.DTOs.OwnerDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/owner")]
    [Authorize(Roles = "owner,admin")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IOwnerService _ownerService;

        public OwnerController(IOwnerService owner)
        {
            _ownerService = owner;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OwnerWithIdDTO>> GetById(int id)
        {
            return await _ownerService.GetById(id);
        }

        [HttpGet]
        public async Task<ActionResult<List<OwnerWithIdDTO>>> GetAll()
        {
            return await _ownerService.GetAll();
        }

        [HttpPost]
        public async Task<ActionResult<OwnerWithoutIdDTO>> Create(OwnerWithoutIdDTO owner)
        {
            return await _ownerService.Create(owner);
        }

        [HttpPut]
        public async Task Update(OwnerWithIdDTO owner)
        {
            await _ownerService.Update(owner);
        }

        [HttpDelete]
        public async Task DeleteById(int id)
        {
            await _ownerService.DeleteById(id);
        }
    }
}
