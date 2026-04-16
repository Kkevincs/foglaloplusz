using Foglaloplusz.BLL.DTOs.GuestDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IGuestService
    {
        Task<GuestWithIdDTO?> GetByIdAsync(int id);
        Task<GuestWithoutIdDTO> CreateAsync(GuestWithoutIdDTO guest);
    }
}
