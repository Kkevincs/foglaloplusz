using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.AdminDTOs;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService admin)
        {
            _adminService = admin;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminWithIdDTO>> GetById(int id)
        {
            return await _adminService.GetById(id);
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminWithIdDTO>>> GetAll()
        {
            return await _adminService.GetAll();
        }

        [HttpPost]
        public async Task<ActionResult<AdminWithoutIdDTO>> Create(AdminWithoutIdDTO admin)
        {
            return await _adminService.Create(admin);
        }

        [HttpPut]
        public async Task Update(AdminWithIdDTO admin)
        {
            await _adminService.Update(admin);
        }

        [HttpDelete]
        public async Task DeleteById(int id)
        {
            await _adminService.DeleteById(id);
        }
    }
}
