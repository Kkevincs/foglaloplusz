using Microsoft.EntityFrameworkCore;
using Foglaloplusz.DAL.Data;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.DAL.Repositories
{
    public class GuestRepository : IGuestRepository
    {
        private readonly AppDbContext _context;

        public GuestRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Guest?> GetByIdAsync(int id) =>
            _context.Guests.FirstOrDefaultAsync(x => x.Id == id);

        public async Task AddAsync(Guest guest)
        {
            guest.Id = 0;
            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();
        }
    }
}
