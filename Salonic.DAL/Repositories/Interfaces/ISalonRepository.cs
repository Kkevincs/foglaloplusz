using Salonic.DAL.Models;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface ISalonRepository
    {
        Task<List<Salon>> ListByOwnerIdAsync(int ownerId);
        Task<List<Salon>> ListAllAsync();
        Task<bool> ExistsByIdAsync(int salonId);
        //Task<string?> GetThemeBySalonIdAsync(int salonId);
        Task<Salon?> GetBySalonIdAsync(int salonId);
        Task<Salon?> GetByOwnerAndIdAsync(int ownerId, int salonId);
        Task AddAsync(Salon salon);
        Task<bool> DeleteCascadeByOwnerAsync(int ownerId, int salonId, CancellationToken cancellationToken = default);
    }
}
