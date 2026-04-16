using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Repositories.Interfaces
{
    public interface IGuestRepository
    {
        Task<Guest?> GetByIdAsync(int id);
        Task AddAsync(Guest guest);
    }
}
