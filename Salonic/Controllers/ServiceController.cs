using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using System.Security.Cryptography.X509Certificates;

namespace Salonic.presentation.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {

        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService service)
        {
            _serviceService = service;
        }
        [Authorize(Roles = "admin,owner,artist,guest")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceWithIdDTO>> GetById(int id) {
            return await _serviceService.GetById(id);
        }
        [Authorize(Roles = "admin,owner,artist,guest")]
        [HttpGet]

        public async Task<ActionResult<List<ServiceWithIdDTO>>> GetAll() {
            return await _serviceService.GetAll();
        }
        [Authorize(Roles = "admin,owner,artist")]
        [HttpPost]
        public async Task<ActionResult<ServiceWithIdDTO>> Create(ServiceWithIdDTO service) {
            return await _serviceService.Create(service);
        }
        [Authorize(Roles = "admin,owner,artist")]
        [HttpPut]

        public async Task Update(ServiceWithIdDTO service) {
            await _serviceService.Update(service);
        }
        [Authorize(Roles = "admin,owner,artist")]
        [HttpDelete]

        public async Task DeleteById(int id) { 
            await _serviceService.DeleteById(id);
        }
    }
}
