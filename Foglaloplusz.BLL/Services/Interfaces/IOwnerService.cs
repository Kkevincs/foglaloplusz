using Foglaloplusz.BLL.DTOs.OwnerDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IOwnerService
    {
        Task<OwnerWithIdDTO?> GetByIdAsync(int id);
        Task<OwnerWithoutIdDTO> CreateAsync(OwnerWithoutIdDTO owner);
    }
}
