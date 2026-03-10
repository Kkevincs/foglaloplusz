using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        public Task<List<Appointment>> GetAll();
        public Task<Appointment> GetById(int id);
        public Task<Appointment> Create(Appointment appointment);
        public Task Update(Appointment artist);
        public Task DeleteById(int id);
    }
}
