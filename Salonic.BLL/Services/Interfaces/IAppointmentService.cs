using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IAppointmentService
    {
        public Task<List<AppointmentsWithIdDTO>> GetAll();
        public Task<AppointmentsWithIdDTO> GetById(int id);
        public Task<AppointmentsWithoutIdDTO> Create(AppointmentsWithoutIdDTO appointment);
        public Task Update(AppointmentsWithIdDTO appointment);
        public Task DeleteById(int id);
    }
}
