using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
                   .Select(u => new { u.Id, u.Name, u.Email })
                   .ToList();
            return Ok(users);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Models.Guest user) {
            _context.Guests.Add(user);
            _context.SaveChanges();
            return Ok(user);    
        }

        [HttpDelete]

        public IActionResult Delete(int id) {
            var user = _context.Guests.Where(x => x.Id == id).First();
            _context.Guests.Remove(user);
            return Ok(user);
        }
        
    }
}
