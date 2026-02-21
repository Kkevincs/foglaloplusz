using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Salonic.Data;
using Salonic.Models;

namespace Salonic.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Get()
        {
            var users = _context.Guests
                   .Select(u => new { u.Id, u.Name, u.Email, u.Pwd })
                   .ToList();
            return Ok(users);
        }

        [HttpGet("{email}")]
        public IActionResult GetByEmail(string email)
        {
            var user = _context.Guests.Where(x => x.Email == email).First();
            if (user == null)
                return NotFound(new { message = "Guest not found" });
            return Ok(user);

        }

        [HttpPost]
        public IActionResult Post([FromBody] Models.Guest user) {
            _context.Guests.Add(user);
            _context.SaveChanges();
            return Ok(user);    
        }

        [HttpDelete]
        public IActionResult Delete(int id) {
            var guest = _context.Guests
                .Include(g => g.Appointments)
                .FirstOrDefault(x => x.Id == id);
            
            if (guest == null)
                return NotFound(new { message = "Guest not found" });

            // Delete all appointments for this guest first
            if (guest.Appointments != null && guest.Appointments.Any())
            {
                _context.Appointments.RemoveRange(guest.Appointments);
            }

            // Now delete the guest
            _context.Guests.Remove(guest);
            _context.SaveChanges();
            
            return Ok(new { message = "Guest and associated appointments deleted successfully", guest });
        }

        
    }
}
