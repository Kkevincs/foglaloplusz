using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.Data;

namespace Salonic.Controllers
{
    [Route("api/appointment")]
    [ApiController]


    public class AppointmentController : ControllerBase
    {

        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context) { 
            _context = context;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Models.Appointment app) {
            _context.Appointments.Add(app);
            _context.SaveChanges();
            return Ok(app);
        }
    }
}
