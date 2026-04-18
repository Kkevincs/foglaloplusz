using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Repositories.Interfaces
{
    public interface IOfferingRepository
    {
        Task<Offering?> GetByIdAsync(int id);
        Task<Offering?> GetByArtistAndServiceAsync(int artistId, int serviceId);
        Task<List<Offering>> ListByArtistWithServiceAsync(int artistId);
        Task<bool> ExistsAsync(int artistId, int serviceId);
        /// <summary>True if the artist has an offering row for every given service id.</summary>
        Task<bool> ArtistOffersAllServicesAsync(int artistId, IReadOnlyCollection<int> serviceIds);
        /// <summary>Artist ids in the salon who have an offering for every service id (services must belong to the salon).</summary>
        Task<List<int>> ListArtistIdsOfferingAllServicesInSalonAsync(int salonId, IReadOnlyCollection<int> serviceIds);
        Task AddAsync(Offering offering);
        Task<bool> DeleteAsync(int offeringId);
        Task<bool> DeleteByArtistAndServiceAsync(int artistId, int serviceId);
    }
}
