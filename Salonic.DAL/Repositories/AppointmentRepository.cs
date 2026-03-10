using Salonic.DAL.Data;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {

        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context) {
            _context = context;
        }
        public async Task<Appointment> Create(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            _context.SaveChanges();
            return appointment;
            
        }

        public async Task DeleteById(int id)
        {
            var app = _context.Appointments.Where(x => x.Id == id).FirstOrDefault();
            _context.Appointments.Remove(app);
            _context.SaveChanges();
            
        }

        public async Task<List<Appointment>> GetAll()
        {
            return _context.Appointments.Select(x => x).ToList();

            
        }

        public async Task<Appointment> GetById(int id)
        {
            return _context.Appointments.Where(x => x.Id == id).FirstOrDefault();
        }

        public async Task Update(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }
    }
}
