using Microsoft.EntityFrameworkCore;
using Salonic.DAL.Data;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic.DAL.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Service[]> GetByIds(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Array.Empty<Service>();

            return await _context.Services
                .Where(x => ids.Contains(x.Id))
                .ToArrayAsync();
        }

        public async Task<List<(int Id, string Name)>> GetIdAndNameBySalonAndArtistIds(int salonId, int artistId, int[] serviceIds)
        {
            if (serviceIds == null || serviceIds.Length == 0)
                return new List<(int, string)>();

            var distinct = serviceIds.Where(x => x > 0).Distinct().ToArray();
            if (distinct.Length == 0)
                return new List<(int, string)>();

            var rows = await _context.Services
                .AsNoTracking()
                .Where(s =>
                    s.SalonId == salonId &&
                    s.CreatedByArtistId == artistId &&
                    distinct.Contains(s.Id))
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var map = rows.ToDictionary(r => r.Id, r => r.Name ?? string.Empty);
            return distinct.Where(id => map.ContainsKey(id)).Select(id => (id, map[id])).ToList();
        }

        public async Task<List<Service>> GetAllBySalonId(int salonId)
        {
            return await _context.Services
                .AsNoTracking()
                .Where(s => s.SalonId == salonId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<bool> ArtistBelongsToSalon(int artistId, int salonId)
        {
            return await _context.Artists.AnyAsync(a => a.Id == artistId && a.SalonId == salonId);
        }

        public async Task<Service?> GetForArtist(int artistId, int salonId, int serviceId)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.Id == serviceId &&
                    s.SalonId == salonId &&
                    s.CreatedByArtistId == artistId);
        }

        public async Task<Service> CreateForArtist(int artistId, Service service)
        {
            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.Id == artistId)
                ?? throw new InvalidOperationException("Artist was not found.");

            if (service.SalonId != artist.SalonId)
                throw new InvalidOperationException("Salon does not match the artist's salon.");

            service.CreatedByArtistId = artistId;
            service.Id = 0;
            service.Appointments = new List<Appointment>();
            service.Artists = new List<Artist>();

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task UpdateForArtist(int artistId, Service incoming)
        {
            var artist = await _context.Artists.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == artistId)
                ?? throw new InvalidOperationException("Artist was not found.");

            var existing = await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.Id == incoming.Id &&
                    s.CreatedByArtistId == artistId)
                ?? throw new InvalidOperationException("Service was not found.");

            if (existing.SalonId != artist.SalonId)
                throw new InvalidOperationException("Service was not found.");

            existing.Name = incoming.Name;
            existing.Description = incoming.Description;
            existing.Price = incoming.Price;
            existing.DurationInMinute = incoming.DurationInMinute;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteForArtistInSalon(int artistId, int salonId, int serviceId)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s =>
                s.Id == serviceId &&
                s.SalonId == salonId &&
                s.CreatedByArtistId == artistId);

            if (service == null)
                return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArtistCanUseAllServices(int artistId, int[] serviceIds)
        {
            if (serviceIds == null || serviceIds.Length == 0)
                return false;

            var artist = await _context.Artists.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == artistId);
            if (artist == null)
                return false;

            var distinct = serviceIds.Distinct().ToArray();
            var count = await _context.Services.CountAsync(s =>
                distinct.Contains(s.Id) &&
                s.SalonId == artist.SalonId &&
                s.CreatedByArtistId == artistId);

            return count == distinct.Length;
        }
    }
}
