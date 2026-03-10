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

        [HttpGet("{id}")]
        public async Task<ActionResult<GetService>> GetById(int id) {
            return await _serviceService.GetById(id);
        }
        [HttpGet]

        public async Task<ActionResult<List<GetService>>> GetAll() {
            return await _serviceService.GetAll();
        }
        [HttpPost]
        public async Task<ActionResult<GetService>> Create(GetService service) {
            return await _serviceService.Create(service);
        }

        [HttpPut]

        public async Task Update(GetService service) {
            await _serviceService.Update(service);
        }

        [HttpDelete]

        public async Task DeleteById(int id) { 
            await _serviceService.DeleteById(id);
        }
    }
}
