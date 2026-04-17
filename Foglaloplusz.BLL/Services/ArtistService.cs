using AutoMapper;
using Foglaloplusz.BLL.DTOs.ArtistDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IMapper _mapper;
        private readonly IArtistRepository _artistRepository;
        private readonly ISalonRepository _salonRepository;

        public ArtistService(IArtistRepository repo, ISalonRepository salonRepository, IMapper mapper)
        {
            _mapper = mapper;
            _artistRepository = repo;
            _salonRepository = salonRepository;
        }

        public async Task<ArtistWithoutIdDTO> CreateAsync(ArtistWithoutIdDTO artist)
        {
            var entity = _mapper.Map<Artist>(artist);
            await _artistRepository.AddAsync(entity);
            return artist;
        }

        public async Task<List<ArtistWithIdDTO>> ListAllAsync()
        {
            var artists = await _artistRepository.ListAllAsync();
            return _mapper.Map<List<ArtistWithIdDTO>>(artists);
        }

        public async Task<List<ArtistWithIdDTO>?> ListBySalonForOwnerAsync(int ownerId, int salonId)
        {
            if (ownerId <= 0 || salonId <= 0)
                return null;

            var salon = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId);
            if (salon == null)
                return null;

            var artists = await _artistRepository.ListBySalonIdAsync(salonId);
            return _mapper.Map<List<ArtistWithIdDTO>>(artists);
        }

        public async Task<List<ArtistWithIdDTO>> ListBySalonForPublicAsync(int salonId)
        {
            if (salonId <= 0)
                return new List<ArtistWithIdDTO>();

            if (!await _salonRepository.ExistsByIdAsync(salonId))
                return new List<ArtistWithIdDTO>();

            var artists = await _artistRepository.ListBySalonIdAsync(salonId);
            return _mapper.Map<List<ArtistWithIdDTO>>(artists);
        }

        public async Task<ArtistWithIdDTO?> GetByIdAsync(int id)
        {
            var artist = await _artistRepository.GetByIdAsync(id);
            return artist == null ? null : _mapper.Map<ArtistWithIdDTO>(artist);
        }
    }
}
