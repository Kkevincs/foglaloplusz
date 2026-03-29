using Microsoft.EntityFrameworkCore;
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

        public async Task DeleteById(int id)
        {
            var app = _context.Appointments.Where(x => x.Id == id).FirstOrDefault();
            _context.Appointments.Remove(app);
            _context.SaveChanges();
            
        }

        public async Task<List<DateTime>> GetDaysWithAppointments(int artistId, DateTime start, DateTime end)
        {
            return await _context.Appointments
                .Where(a =>
                    a.ArtistId == artistId &&
                    a.StartTime >= start &&
                    a.StartTime <= end)
                .Select(a => a.StartTime.Date)
                .Distinct()
                .ToListAsync(); //eg: 2025-03-01
        }

        public async Task<List<Appointment>> GetAll()
        {
            return await _context.Appointments
                .Include(a => a.Services)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByArtistAndDate(int artistId, DateTime date)
        {
            return await _context.Appointments
            .Include(a => a.Services)
            .Where(x => x.ArtistId == artistId && x.StartTime.Date == date.Date)
            .OrderBy(x => x.StartTime)
            .ToListAsync();
        }

        public async Task<List<Appointment>> GetByArtistId(int id)
        {
            return await _context.Appointments
                .Include(a => a.Services)
                .Where(x => x.ArtistId == id)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByGuestId(int id)
        {
            return await _context.Appointments
                .Include(a => a.Services)
                .Where(x => x.GuestId == id)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetBySalonId(int salonId)
        {
            return await _context.Appointments
                .Include(a => a.Services)
                .Where(a => a.SalonId == salonId)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<Appointment> GetById(int id)
        {
            return await _context.Appointments
                .Include(a => a.Services)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsOverlapping(int artistId, DateTime startTime, DateTime endTime)
        {
            return await _context.Appointments.AnyAsync(a =>
            a.ArtistId == artistId &&
            startTime < a.EndTime &&
            endTime > a.StartTime);
        }

        public async Task Update(Appointment appointment)
        {
            var existing = await _context.Appointments
                .Include(a => a.Services)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            existing.GuestId = appointment.GuestId;
            existing.ArtistId = appointment.ArtistId;
            existing.StartTime = appointment.StartTime;
            existing.EndTime = appointment.EndTime;
            existing.Notes = appointment.Notes;
            existing.SalonId = appointment.SalonId;
            await _context.SaveChangesAsync();
        }

        public async Task Create(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }
    }
}
