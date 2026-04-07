using Salonic.DAL.Models;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface ISalonRepository
    {
        Task<List<Salon>> ListByOwnerIdAsync(int ownerId);
        Task<bool> ExistsByIdAsync(int salonId);
        /// <summary>Theme class string for the salon, or null if no salon with this id.</summary>
        Task<string?> GetThemeBySalonIdAsync(int salonId);
        Task<Salon?> GetByOwnerAndIdAsync(int ownerId, int salonId);
        Task AddAsync(Salon salon);

        /// <summary>Deletes the salon and all related appointments, services, working hours, and artists. Returns false if not found or not owned.</summary>
        Task<bool> DeleteCascadeByOwnerAsync(int ownerId, int salonId, CancellationToken cancellationToken = default);
    }
}
