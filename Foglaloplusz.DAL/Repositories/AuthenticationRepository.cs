using Microsoft.EntityFrameworkCore;
using Foglaloplusz.DAL.Data;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.DAL.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly AppDbContext _context;

        public AuthenticationRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Guest?> FindGuestByCredentials(string email, string password) =>
            _context.Guests.FirstOrDefaultAsync(x => x.Email == email && x.Pwd == password);

        public Task<Artist?> FindArtistByCredentials(string email, string password) =>
            _context.Artists.FirstOrDefaultAsync(x => x.Email == email && x.Pwd == password);

        public Task<Owner?> FindOwnerByCredentials(string email, string password) =>
            _context.Owners.FirstOrDefaultAsync(x => x.Email == email && x.Pwd == password);

        public async Task<bool> EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalizedEmail = email.Trim();

            return await _context.Guests.AnyAsync(x => x.Email == normalizedEmail)
                || await _context.Artists.AnyAsync(x => x.Email == normalizedEmail)
                || await _context.Owners.AnyAsync(x => x.Email == normalizedEmail);
        }
    }
}
