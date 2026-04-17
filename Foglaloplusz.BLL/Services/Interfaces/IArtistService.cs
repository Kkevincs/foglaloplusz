using Foglaloplusz.BLL.DTOs.ArtistDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IArtistService
    {
        Task<List<ArtistWithIdDTO>> ListAllAsync();
        Task<List<ArtistWithIdDTO>?> ListBySalonForOwnerAsync(int ownerId, int salonId);
        /// <summary>Artists visible for booking on the public salon site (no auth).</summary>
        Task<List<ArtistWithIdDTO>> ListBySalonForPublicAsync(int salonId);
        Task<ArtistWithIdDTO?> GetByIdAsync(int id);
        Task<ArtistWithoutIdDTO> CreateAsync(ArtistWithoutIdDTO artist);
    }
}
