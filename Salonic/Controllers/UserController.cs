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
        public IActionResult get()
        {
            var users = _context.Users
                   .Select(u => new { u.ID, u.Email })
                   .ToList();
            return Ok(users);
        }

        [HttpPost]
        public IActionResult post([FromBody] Models.User user) {
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);    
        }

        [HttpDelete]

        public IActionResult delete(int id) {
            var user = _context.Users.Where(x => x.ID == id).First();
            _context.Users.Remove(user);
            return Ok(user);
        }
        
    }
}
