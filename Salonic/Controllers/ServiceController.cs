using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.Data;

namespace Salonic.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var apps = _context.Services.Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.Price,
                //x.WorkerId
            }).ToList();
            return Ok(apps);
        }
        [HttpGet]
        
        public async Task<ActionResult<List<ServiceWithIdDTO>>> GetAll() {
            return await _serviceService.GetAll();
        }
    }
}
    
