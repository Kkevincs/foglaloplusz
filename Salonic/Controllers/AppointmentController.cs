using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/appointment")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AppointmentsWithIdDTO>>> GetAll()
        { 
            return await _appointmentService.GetAll();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentsWithIdDTO>> GetById(int id)
        { 
            return await _appointmentService.GetById(id);
        }

        [HttpPost]

        public async Task<ActionResult<AppointmentsWithoutIdDTO>> Create(AppointmentsWithoutIdDTO app)
        {
            return await _appointmentService.Create(app);
        }

        [HttpPut]

        public async Task Update(AppointmentsWithIdDTO app) { 
            await _appointmentService.Update(app);
        }

        [HttpDelete]
        public async Task DeleteById(int id) { 
            await _appointmentService.DeleteById(id);
        }


    }
}
