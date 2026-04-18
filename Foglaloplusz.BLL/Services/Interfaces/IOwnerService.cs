using Foglaloplusz.BLL.DTOs.OwnerDTOs;
using Foglaloplusz.BLL.DTOs.UserDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IOwnerService
    {
        Task<OwnerWithIdDTO?> GetByIdAsync(int id);
        Task<OwnerWithIdDTO> GetMeOrThrowAsync(int ownerId);
        Task<OwnerWithoutIdDTO> CreateAsync(OwnerWithoutIdDTO owner);
        Task<OwnerWithoutIdDTO> RegisterAsync(OwnerWithoutIdDTO? owner);
        Task<OwnerWithIdDTO> UpdateProfileAsync(int ownerId, UserProfileUpdateDTO dto);
    }
}
