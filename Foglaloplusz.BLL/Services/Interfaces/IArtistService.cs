using Foglaloplusz.BLL.DTOs.ArtistDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IArtistService
    {
        Task<List<ArtistWithIdDTO>> ListAllAsync();
        Task<List<ArtistWithIdDTO>> ListBySalonForOwnerAsync(int ownerId, int salonId);
        /// <summary>Artists visible for booking on the public salon site (no auth).</summary>
        /// <param name="mustOfferServiceIds">Ha megadott és nem üres: csak olyan szakemberek, akiknek az offeringjeiben mindegyik szolgáltatás szerepel.</param>
        Task<List<ArtistWithIdDTO>> ListBySalonForPublicAsync(int salonId, int[]? mustOfferServiceIds = null);
        Task<ArtistWithIdDTO?> GetByIdAsync(int id);
        Task<ArtistWithIdDTO> GetMeOrThrowAsync(int artistId);
        Task<ArtistWithoutIdDTO> CreateAsync(ArtistWithoutIdDTO artist);
        Task<ArtistWithoutIdDTO> RegisterAsync(ArtistWithoutIdDTO? artist);
    }
}
