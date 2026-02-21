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
            // Remove navigation property errors from ModelState since we handle them manually
            ModelState.Remove(nameof(app.Guest));
            ModelState.Remove(nameof(app.Worker));
            ModelState.Remove(nameof(app.Service));
            
            // Validate scalar properties only
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Validate and fetch related entities
            var guest = _context.Guests.Find(app.GuestId);
            if (guest == null)
                return BadRequest(new { message = "Guest not found" });

            var worker = _context.Workers.Find(app.WorkerId);
            if (worker == null)
                return BadRequest(new { message = "Worker not found" });

            var service = _context.Services.Find(app.ServiceId);
            if (service == null)
                return BadRequest(new { message = "Service not found" });

            var appointment = new Models.Appointment
            {
                GuestId = app.GuestId,
                WorkerId = app.WorkerId,
                ServiceId = app.ServiceId,
                Year = app.Year,
                Month = app.Month,
                Day = app.Day,
                Hour = app.Hour,
                Minute = app.Minute,
                Notes = string.IsNullOrEmpty(app.Notes) ? "" : app.Notes,
                DurationInMinutes = app.DurationInMinutes,
                Guest = guest,
                Worker = worker,
                Service = service
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();
            return Ok(appointment);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var bookings = _context.Appointments.Select(x => x);
            return Ok(bookings);
        }
    }
}
