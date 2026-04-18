using Microsoft.EntityFrameworkCore;
using Foglaloplusz.DAL.Data;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.DAL.Repositories
{
    public class OfferingRepository : IOfferingRepository
    {
        private readonly AppDbContext _context;

        public OfferingRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Offering?> GetByIdAsync(int id) =>
            _context.Offerings
                .Include(o => o.Service)
                .FirstOrDefaultAsync(o => o.Id == id);

        public Task<Offering?> GetByArtistAndServiceAsync(int artistId, int serviceId) =>
            _context.Offerings
                .Include(o => o.Service)
                .FirstOrDefaultAsync(o => o.ArtistId == artistId && o.ServiceId == serviceId);

        public Task<List<Offering>> ListByArtistWithServiceAsync(int artistId) =>
            _context.Offerings
                .AsNoTracking()
                .Include(o => o.Service)
                .Where(o => o.ArtistId == artistId)
                .OrderBy(o => o.Service!.Name)
                .ToListAsync();

        public Task<bool> ExistsAsync(int artistId, int serviceId) =>
            _context.Offerings.AnyAsync(o => o.ArtistId == artistId && o.ServiceId == serviceId);

        public async Task<bool> ArtistOffersAllServicesAsync(int artistId, IReadOnlyCollection<int> serviceIds)
        {
            var set = serviceIds?.Where(id => id > 0).Distinct().ToHashSet() ?? new HashSet<int>();
            if (set.Count == 0)
                return false;
            var count = await _context.Offerings
                .Where(o => o.ArtistId == artistId && set.Contains(o.ServiceId))
                .Select(o => o.ServiceId)
                .Distinct()
                .CountAsync();
            return count == set.Count;
        }

        public async Task<List<int>> ListArtistIdsOfferingAllServicesInSalonAsync(int salonId, IReadOnlyCollection<int> serviceIds)
        {
            var set = serviceIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (set.Count == 0)
                return new List<int>();

            var validInSalon = await _context.Services
                .AsNoTracking()
                .CountAsync(s => s.SalonId == salonId && set.Contains(s.Id));
            if (validInSalon != set.Count)
                return new List<int>();

            var artistIds = await _context.Artists
                .AsNoTracking()
                .Where(a => a.SalonId == salonId)
                .Select(a => a.Id)
                .ToListAsync();

            var eligible = new List<int>();
            foreach (var aId in artistIds)
            {
                if (await ArtistOffersAllServicesAsync(aId, set))
                    eligible.Add(aId);
            }

            return eligible;
        }

        public async Task AddAsync(Offering offering)
        {
            offering.Id = 0;
            _context.Offerings.Add(offering);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int offeringId)
        {
            var o = await _context.Offerings.FirstOrDefaultAsync(x => x.Id == offeringId);
            if (o == null)
                return false;
            _context.Offerings.Remove(o);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByArtistAndServiceAsync(int artistId, int serviceId)
        {
            var o = await _context.Offerings.FirstOrDefaultAsync(x => x.ArtistId == artistId && x.ServiceId == serviceId);
            if (o == null)
                return false;
            _context.Offerings.Remove(o);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
