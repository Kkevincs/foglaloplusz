using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdAsync(int id);
        Task<Service[]> GetByIdsAsync(int[] ids);
        Task<List<Service>> ListBySalonIdAsync(int salonId);
        /// <summary>Csak olyan szolgáltatások, amelyekhez van legalább egy Offering sor.</summary>
        Task<List<Service>> ListBySalonIdWithOfferingsAsync(int salonId);
        Task AddAsync(Service service);
        Task UpdateAsync(Service incoming);
        Task<bool> DeleteAsync(int id);
    }
}
