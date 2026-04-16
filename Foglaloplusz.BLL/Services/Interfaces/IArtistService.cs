using Foglaloplusz.BLL.DTOs.ArtistDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IArtistService
    {
        Task<List<ArtistWithIdDTO>> ListAllAsync();
        Task<List<ArtistWithIdDTO>?> ListBySalonForOwnerAsync(int ownerId, int salonId);
        Task<ArtistWithIdDTO?> GetByIdAsync(int id);
        Task<ArtistWithoutIdDTO> CreateAsync(ArtistWithoutIdDTO artist);
    }
}
