using Salonic.BLL.DTOs.GuestDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IGuestService
    {
        Task<GuestWithIdDTO?> GetByIdAsync(int id);
        Task<GuestWithoutIdDTO> CreateAsync(GuestWithoutIdDTO guest);
    }
}
