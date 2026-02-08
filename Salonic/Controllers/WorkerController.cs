using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.Data;

namespace Salonic.Controllers
{
    [Route("api/worker")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private readonly AppDbContext _context;
        public WorkerController(AppDbContext context) { 
            _context = context;
        
        }

        [HttpGet]
        public IActionResult Get()
        { 
            var workers = _context.Workers.Select(x => new { 
                x.ID,
                x.Name,
                x.Email
            }).ToList();
            return(Ok (workers));
        }
    }
}
