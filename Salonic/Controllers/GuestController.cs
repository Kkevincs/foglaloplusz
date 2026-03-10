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

        [HttpGet("{id}")]
        public async Task<ActionResult<GetGuest>> GetById(int id)
        {
            return await _guestService.GetById(id);
        }
        [HttpGet]

        public async Task<ActionResult<List<GetGuest>>> GetAll()
        {
            return await _guestService.GetAll();
        }
        [HttpPost]
        public async Task<ActionResult<AddGuest>> Create(AddGuest guest)
        {
            return await _guestService.Create(guest);
        }

        [HttpPut]

        public async Task Update(GetGuest guest)
        {
            await _guestService.Update(guest);
        }

        [HttpDelete]

        public async Task DeleteById(int id)
        {
            await _guestService.DeleteById(id);
        }
    }
}
