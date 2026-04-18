using Microsoft.EntityFrameworkCore;
using Foglaloplusz.DAL.Data;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.DAL.Repositories
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Artist>> ListAllAsync() =>
            _context.Artists.OrderBy(a => a.Name).ToListAsync();

        public Task<List<Artist>> ListBySalonIdAsync(int salonId) =>
            _context.Artists
                .Where(a => a.SalonId == salonId)
                .OrderBy(a => a.Name)
                .ToListAsync();

        public Task<Artist?> GetByIdAsync(int id) =>
            _context.Artists.FirstOrDefaultAsync(x => x.Id == id);

        public async Task AddAsync(Artist artist)
        {
            artist.Id = 0;
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Artist _) =>
            _context.SaveChangesAsync();
    }
}
