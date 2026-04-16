using Microsoft.EntityFrameworkCore;
using Foglaloplusz.DAL.Data;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.DAL.Repositories
{
    public class SalonRepository : ISalonRepository
    {
        private readonly AppDbContext _context;

        public SalonRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Salon>> ListAllOrderedByNameAsync() =>
            _context.Salons
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

        public Task<List<Salon>> ListByOwnerIdAsync(int ownerId) =>
            _context.Salons
                .Where(s => s.OwnerId == ownerId)
                .OrderBy(s => s.Name)
                .ToListAsync();

        public Task<List<Salon>> ListAllAsync() =>
            _context.Salons
                .OrderBy(s => s.Name)
                .ToListAsync();

        public Task<bool> ExistsByIdAsync(int salonId) =>
            _context.Salons.AnyAsync(s => s.Id == salonId);

        //public async Task<string?> GetThemeBySalonIdAsync(int salonId)
        //{
        //    var row = await _context.Salons
        //        .AsNoTracking()
        //        .Where(s => s.Id == salonId)
        //        .Select(s => new { s.Theme })
        //        .FirstOrDefaultAsync();
        //    if (row == null)
        //        return null;
        //    return row.Theme ?? string.Empty;
        //}
        
        public Task<Salon?> GetBySalonIdAsync(int salonId) =>
            _context.Salons
                .FirstOrDefaultAsync(s => s.Id == salonId);

        public Task<Salon?> GetByOwnerAndIdAsync(int ownerId, int salonId) =>
            _context.Salons
                .FirstOrDefaultAsync(s => s.Id == salonId && s.OwnerId == ownerId);

        public async Task AddAsync(Salon salon)
        {
            salon.Id = 0;
            _context.Salons.Add(salon);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteCascadeByOwnerAsync(int ownerId, int salonId, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var salon = await _context.Salons
                .FirstOrDefaultAsync(s => s.Id == salonId && s.OwnerId == ownerId, cancellationToken);

            if (salon == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            var artistIds = await _context.Artists
                .Where(a => a.SalonId == salonId)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            var appointments = await _context.Appointments
                .Where(a => a.SalonId == salonId || artistIds.Contains(a.ArtistId))
                .Include(a => a.Services)
                .ToListAsync(cancellationToken);

            foreach (var a in appointments)
                a.Services.Clear();

            _context.Appointments.RemoveRange(appointments);

            if (artistIds.Count > 0)
            {
                var workingHours = await _context.WorkingHours
                    .Where(w => artistIds.Contains(w.ArtistId))
                    .ToListAsync(cancellationToken);
                _context.WorkingHours.RemoveRange(workingHours);
            }

            var services = await _context.Services
                .Where(s => s.SalonId == salonId)
                .Include(s => s.Artists)
                .Include(s => s.Appointments)
                .ToListAsync(cancellationToken);

            foreach (var s in services)
            {
                s.Artists.Clear();
                s.Appointments.Clear();
            }

            _context.Services.RemoveRange(services);

            var artists = await _context.Artists
                .Where(a => a.SalonId == salonId)
                .ToListAsync(cancellationToken);
            _context.Artists.RemoveRange(artists);

            _context.Salons.Remove(salon);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
    }
}
