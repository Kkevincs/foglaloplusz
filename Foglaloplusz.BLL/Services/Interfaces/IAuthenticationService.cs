using Foglaloplusz.BLL.DTOs.UserDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<string?> SignInAsync(UserDTO? credentials);
        Task<bool> EmailExistsAsync(string email);
        /// <summary>Bejelentkezési kérés ellenőrzése — hiba esetén AppValidationException.</summary>
        void EnsureValidLoginRequest(UserDTO? authInfo);
    }
}
