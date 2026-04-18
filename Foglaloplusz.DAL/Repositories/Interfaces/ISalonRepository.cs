using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Repositories.Interfaces
{
    public interface ISalonRepository
    {
        /// <summary>All salons, ordered by name (public directory).</summary>
        Task<List<Salon>> ListAllOrderedByNameAsync();

        Task<List<Salon>> ListByOwnerIdAsync(int ownerId);
        Task<bool> ExistsByIdAsync(int salonId);
        Task<Salon?> GetBySalonIdAsync(int salonId);
        Task<Salon?> GetByOwnerAndIdAsync(int ownerId, int salonId);
        Task AddAsync(Salon salon);
        /// <summary>Tracked entitás módosításainak mentése.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> DeleteCascadeByOwnerAsync(int ownerId, int salonId, CancellationToken cancellationToken = default);
    }
}
