using Salonic.DAL.Models;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IGuestRepository
    {
        Task<Guest?> GetByIdAsync(int id);
        Task AddAsync(Guest guest);
    }
}
