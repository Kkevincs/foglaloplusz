using Microsoft.EntityFrameworkCore;
using Foglaloplusz.DAL.Data;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.DAL.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Service?> GetByIdAsync(int id) =>
            _context.Services.FirstOrDefaultAsync(s => s.Id == id);

        public async Task<Service[]> GetByIdsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Array.Empty<Service>();

            return await _context.Services
                .Where(x => ids.Contains(x.Id))
                .ToArrayAsync();
        }

        public Task<List<Service>> ListBySalonIdAsync(int salonId) =>
            _context.Services
                .Where(s => s.SalonId == salonId)
                .OrderBy(s => s.Name)
                .ToListAsync();

        public Task<List<Service>> ListBySalonIdWithOfferingsAsync(int salonId) =>
            _context.Services
                .AsNoTracking()
                .Where(s => s.SalonId == salonId && s.Offerings.Any())
                .OrderBy(s => s.Name)
                .ToListAsync();

        public async Task AddAsync(Service service)
        {
            ValidateExactlyOneOwnerOrArtist(service);
            service.Id = 0;
            service.Appointments ??= new List<Appointment>();
            service.Offerings ??= new List<Offering>();
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service incoming)
        {
            var existing = await _context.Services.FirstOrDefaultAsync(s => s.Id == incoming.Id);
            if (existing == null)
                return;

            existing.Name = incoming.Name;
            existing.Description = incoming.Description;
            existing.Price = incoming.Price;
            existing.DurationInMinute = incoming.DurationInMinute;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return false;

            _context.Services.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>Ensures exactly one of <see cref="Service.OwnerId"/> or <see cref="Service.ArtistId"/> is set.</summary>
        public static void ValidateExactlyOneOwnerOrArtist(Service service)
        {
            var hasOwner = service.OwnerId.HasValue;
            var hasArtist = service.ArtistId.HasValue;
            if (hasOwner == hasArtist)
                throw new InvalidOperationException("A szolgáltatásnak pontosan egy tulajdonos- vagy szakember-azonosító kell (OwnerId vagy ArtistId).");
        }
    }
}
