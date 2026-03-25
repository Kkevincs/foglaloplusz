using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.DTOs.SalonDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/salon")]
    [ApiController]
    public class SalonController : ControllerBase
    {
        private readonly ISalonService _salonService;

        public SalonController(ISalonService salon)
        {
            _salonService = salon;
        }
        [Authorize(Roles = "admin,owner")]
        [HttpGet("{id}")]
        public async Task<ActionResult<SalonWithIdDTO>> GetById(int id)
        {
            return await _salonService.GetById(id);
        }
        [Authorize(Roles ="admin")]
        [HttpGet]
        public async Task<ActionResult<List<SalonWithIdDTO>>> GetAll()
        {
            return await _salonService.GetAll();
        }
        [Authorize(Roles = "admin,owner")]
        [HttpPost]
        public async Task<ActionResult<SalonWithoutIdDTO>> Create(SalonWithoutIdDTO salon)
        {
            return await _salonService.Create(salon);
        }
        [Authorize(Roles = "admin,owner")]
        [HttpPut]
        public async Task Update(SalonWithIdDTO salon)
        {
            await _salonService.Update(salon);
        }
        [Authorize(Roles = "admin,owner")]
        [HttpDelete]
        public async Task DeleteById(int id)
        {
            await _salonService.DeleteById(id);
        }
    }
}
