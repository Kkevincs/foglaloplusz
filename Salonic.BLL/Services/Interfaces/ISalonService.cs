using Salonic.BLL.DTOs.SalonDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface ISalonService
    {
        Task<List<SalonWithIdDTO>> GetAllForOwner(int ownerId);
        Task<SalonWithIdDTO?> GetByIdForOwner(int ownerId, int salonId);
        Task<SalonWithIdDTO> CreateForOwner(int ownerId, SalonWithoutIdDTO salon);
        Task UpdateForOwner(int ownerId, SalonWithIdDTO salon);
        Task DeleteForOwner(int ownerId, int salonId);
    }
}
