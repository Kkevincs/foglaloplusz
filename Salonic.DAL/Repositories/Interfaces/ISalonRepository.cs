using Salonic.DAL.Models;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface ISalonRepository
    {
        Task<List<Salon>> GetAllByOwnerId(int ownerId);
        Task<Salon?> GetByOwnerAndId(int ownerId, int salonId);
        Task<Salon> CreateForOwner(int ownerId, Salon salon);
        Task UpdateForOwner(int ownerId, Salon incoming);
        Task<bool> DeleteForOwner(int ownerId, int salonId);
    }
}
