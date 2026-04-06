using Microsoft.EntityFrameworkCore;
using Salonic.DAL.Data;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic.DAL.Repositories
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly AppDbContext _context;

        public OwnerRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Owner?> GetByIdAsync(int id) =>
            _context.Owners.FirstOrDefaultAsync(x => x.Id == id);

        public async Task AddAsync(Owner owner)
        {
            owner.Id = 0;
            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();
        }
    }
}
