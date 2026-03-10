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
        public Task<List<GetAppointment>> GetAll();
        public Task<GetAppointment> GetById(int id);
        public Task<AddAppointment> Create(AddAppointment appointment);
        public Task Update(GetAppointment appointment);
        public Task DeleteById(int id);
    }
}
