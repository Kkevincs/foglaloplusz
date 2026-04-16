using Foglaloplusz.BLL.DTOs.SalonDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface ISalonService
    {
        Task<bool> ExistsAsync(int salonId);

        /// <summary>Public list for portal / directory (no owner e-mail).</summary>
        Task<List<SalonDirectoryCardDTO>> ListPublicDirectoryAsync();

        Task<List<SalonWithIdDTO>> ListForOwnerAsync(int ownerId);
        Task<SalonWithIdDTO> CreateForOwnerAsync(int ownerId, SalonWithoutIdDTO salon);
        Task DeleteForOwnerAsync(int ownerId, int salonId, CancellationToken cancellationToken = default);
    }
}
