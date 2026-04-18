using Foglaloplusz.BLL.DTOs.OwnerDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IOwnerService
    {
        Task<OwnerWithIdDTO?> GetByIdAsync(int id);
        Task<OwnerWithIdDTO> GetMeOrThrowAsync(int ownerId);
        Task<OwnerWithoutIdDTO> CreateAsync(OwnerWithoutIdDTO owner);
        Task<OwnerWithoutIdDTO> RegisterAsync(OwnerWithoutIdDTO? owner);
    }
}
