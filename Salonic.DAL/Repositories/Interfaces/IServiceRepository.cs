using Salonic.DAL.Models;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        Task<Service[]> GetByIds(int[] ids);

        Task<List<(int Id, string Name)>> GetIdAndNameBySalonAndArtistIds(int salonId, int artistId, int[] serviceIds);

        Task<List<Service>> GetAllBySalonId(int salonId);

        Task<bool> ArtistBelongsToSalon(int artistId, int salonId);
        Task<Service?> GetForArtist(int artistId, int salonId, int serviceId);
        Task<Service> CreateForArtist(int artistId, Service service);
        Task UpdateForArtist(int artistId, Service incoming);
        Task<bool> DeleteForArtistInSalon(int artistId, int salonId, int serviceId);

        Task<bool> ArtistCanUseAllServices(int artistId, int[] serviceIds);
    }
}
