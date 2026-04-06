using Microsoft.EntityFrameworkCore;
using Salonic.DAL.Data;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic.DAL.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Appointment>> ListByGuestIdAsync(int guestId) =>
            _context.Appointments
                .Include(a => a.Services)
                .Where(x => x.GuestId == guestId)
                .ToListAsync();

        public Task<List<Appointment>> ListBySalonIdAsync(int salonId) =>
            _context.Appointments
                .Include(a => a.Services)
                .Where(a => a.SalonId == salonId)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

        public Task<List<Appointment>> ListByArtistAndDateAsync(int artistId, DateTime date) =>
            _context.Appointments
                .Include(a => a.Services)
                .Where(x => x.ArtistId == artistId && x.StartTime.Date == date.Date)
                .OrderBy(x => x.StartTime)
                .ToListAsync();

        public Task<List<Appointment>> ListByArtistAndDateRangeAsync(
            int artistId,
            DateTime rangeStart,
            DateTime rangeEndExclusive) =>
            _context.Appointments
                .Include(a => a.Services)
                .Where(a =>
                    a.ArtistId == artistId &&
                    a.StartTime < rangeEndExclusive &&
                    a.EndTime > rangeStart)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

        public Task<bool> HasOverlapAsync(int artistId, DateTime startTime, DateTime endTime) =>
            _context.Appointments.AnyAsync(a =>
                a.ArtistId == artistId &&
                startTime < a.EndTime &&
                endTime > a.StartTime);

        public async Task AddAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }
    }
}
