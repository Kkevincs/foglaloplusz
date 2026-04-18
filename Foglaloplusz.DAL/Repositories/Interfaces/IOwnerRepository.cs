using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Repositories.Interfaces
{
    public interface IOwnerRepository
    {
        Task<Owner?> GetByIdAsync(int id);
        Task AddAsync(Owner owner);
        Task UpdateAsync(Owner owner);
    }
}
