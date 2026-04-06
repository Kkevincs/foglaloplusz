using Salonic.BLL.DTOs.OwnerDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IOwnerService
    {
        Task<OwnerWithIdDTO?> GetByIdAsync(int id);
        Task<OwnerWithoutIdDTO> CreateAsync(OwnerWithoutIdDTO owner);
    }
}
