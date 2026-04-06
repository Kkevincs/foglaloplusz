using Salonic.BLL.DTOs.UserDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<string?> SignInAsync(UserDTO credentials);
        Task<bool> EmailExistsAsync(string email);
    }
}
