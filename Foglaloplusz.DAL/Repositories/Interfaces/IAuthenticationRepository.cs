using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Repositories.Interfaces
{
    public interface IAuthenticationRepository
    {
        Task<Guest?> FindGuestByCredentials(string email, string password);
        Task<Artist?> FindArtistByCredentials(string email, string password);
        Task<Owner?> FindOwnerByCredentials(string email, string password);
        Task<bool> EmailExists(string email);
    }
}
