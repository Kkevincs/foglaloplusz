using Salonic.DAL.Models;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdAsync(int id);
        Task<Service[]> GetByIdsAsync(int[] ids);
        Task<List<Service>> ListBySalonIdAsync(int salonId);
        Task<List<Service>> ListByArtistIdAsync(int artistId);
        Task AddAsync(Service service);
        Task UpdateAsync(Service incoming);
        Task<bool> DeleteAsync(int id);
    }
}
