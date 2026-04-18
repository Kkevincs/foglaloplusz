using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Repositories.Interfaces
{
    public interface IArtistRepository
    {
        Task<List<Artist>> ListAllAsync();
        Task<List<Artist>> ListBySalonIdAsync(int salonId);
        Task<Artist?> GetByIdAsync(int id);
        Task AddAsync(Artist artist);
        Task UpdateAsync(Artist artist);
    }
}
