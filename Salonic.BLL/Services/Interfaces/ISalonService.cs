using Salonic.BLL.DTOs.SalonDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface ISalonService
    {
        Task<bool> ExistsAsync(int salonId);
        Task<List<SalonWithIdDTO>> ListForOwnerAsync(int ownerId);
        Task<SalonWithIdDTO> CreateForOwnerAsync(int ownerId, SalonWithoutIdDTO salon);
        Task DeleteForOwnerAsync(int ownerId, int salonId, CancellationToken cancellationToken = default);
    }
}
