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

        public Task<List<DateTime>> GetDaysWithAppointments(int artistId, DateTime start, DateTime end);
        public Task<List<Appointment>> GetByArtistId(int id);

        public Task<List<Appointment>> GetByGuestId(int id);

        Task<List<Appointment>> GetByArtistAndDate(int artistId, DateTime date);
        Task<bool> IsOverlapping(int artistId, DateTime startTime, DateTime endTime);
        Task Create(Appointment appointment);
        public Task Update(Appointment artist);
        public Task DeleteById(int id);
    }
}
