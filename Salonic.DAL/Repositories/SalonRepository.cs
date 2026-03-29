using Microsoft.EntityFrameworkCore;
using Salonic.DAL.Data;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic.DAL.Repositories
{
    public class SalonRepository : ISalonRepository
    {
        private readonly AppDbContext _context;

        public SalonRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Salon>> GetAllByOwnerId(int ownerId)
        {
            return await _context.Salons
                .Where(s => s.OwnerId == ownerId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Salon?> GetByOwnerAndId(int ownerId, int salonId)
        {
            return await _context.Salons
                .FirstOrDefaultAsync(s => s.Id == salonId && s.OwnerId == ownerId);
        }

        public async Task<Salon> CreateForOwner(int ownerId, Salon salon)
        {
            salon.OwnerId = ownerId;
            _context.Salons.Add(salon);
            await _context.SaveChangesAsync();
            return salon;
        }

        public async Task UpdateForOwner(int ownerId, Salon incoming)
        {
            var existing = await GetByOwnerAndId(ownerId, incoming.Id)
                ?? throw new InvalidOperationException("Salon was not found.");

            existing.Name = incoming.Name;
            existing.Email = incoming.Email;
            existing.Description = incoming.Description;
            existing.Theme = incoming.Theme;
            existing.LogoUrl = incoming.LogoUrl;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteForOwner(int ownerId, int salonId)
        {
            var existing = await GetByOwnerAndId(ownerId, salonId);
            if (existing == null)
                return false;

            _context.Salons.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
