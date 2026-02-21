using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                x.Price
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

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var service = _context.Services
                .Include(s => s.Appointments)
                .FirstOrDefault(s => s.Id == id);
            
            if (service == null)
                return NotFound(new { message = "Service not found" });

            // Delete all appointments for this service first
            if (service.Appointments != null && service.Appointments.Any())
            {
                _context.Appointments.RemoveRange(service.Appointments);
            }

            // Now delete the service
            _context.Services.Remove(service);
            _context.SaveChanges();
            
            return Ok(new { message = "Service and associated appointments deleted successfully", service });
        }
    }
}
    
