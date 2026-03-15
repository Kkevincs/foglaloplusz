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

        [HttpGet("{id}")]
        public async Task<ActionResult<SalonWithIdDTO>> GetById(int id)
        {
            return await _salonService.GetById(id);
        }

        [HttpGet]
        public async Task<ActionResult<List<SalonWithIdDTO>>> GetAll()
        {
            return await _salonService.GetAll();
        }

        [HttpPost]
        public async Task<ActionResult<SalonWithoutIdDTO>> Create(SalonWithoutIdDTO salon)
        {
            return await _salonService.Create(salon);
        }

        [HttpPut]
        public async Task Update(SalonWithIdDTO salon)
        {
            await _salonService.Update(salon);
        }

        [HttpDelete]
        public async Task DeleteById(int id)
        {
            await _salonService.DeleteById(id);
        }
    }
}
