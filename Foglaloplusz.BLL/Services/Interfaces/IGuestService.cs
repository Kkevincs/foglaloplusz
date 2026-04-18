using Foglaloplusz.BLL.DTOs.GuestDTOs;
using Foglaloplusz.BLL.DTOs.UserDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IGuestService
    {
        Task<GuestWithIdDTO?> GetByIdAsync(int id);
        Task<GuestWithoutIdDTO> CreateAsync(GuestWithoutIdDTO guest);
        /// <summary>Regisztráció: egyedi e-mail ellenőrzés a BLL-ben.</summary>
        Task<GuestWithoutIdDTO> RegisterAsync(GuestWithoutIdDTO? guest);
        /// <summary>Vendég „én” profil: hiány vagy szerepkör szerinti hiba.</summary>
        Task<GuestWithIdDTO> GetMeOrThrowAsync(int guestId, bool callerIsGuestRole);
        Task<GuestWithIdDTO> UpdateProfileAsync(int guestId, UserProfileUpdateDTO dto);
    }
}
