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
                x.WorkerId
            }).ToList();
            return Ok(apps);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Models.Service service)
        {
            _context.Services.Add(service);
            _context.SaveChanges();
            return Ok(service);
        }
    }
}
    
