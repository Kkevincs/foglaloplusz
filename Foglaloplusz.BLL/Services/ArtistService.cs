using System.Linq;
using AutoMapper;
using Foglaloplusz.BLL.DTOs.ArtistDTOs;
using Foglaloplusz.BLL.Exceptions;
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
        private readonly IOfferingRepository _offeringRepository;
        private readonly IAuthenticationService _authenticationService;

        public ArtistService(
            IArtistRepository repo,
            ISalonRepository salonRepository,
            IOfferingRepository offeringRepository,
            IAuthenticationService authenticationService,
            IMapper mapper)
        {
            _mapper = mapper;
            _artistRepository = repo;
            _salonRepository = salonRepository;
            _offeringRepository = offeringRepository;
            _authenticationService = authenticationService;
        }

        public async Task<ArtistWithoutIdDTO> RegisterAsync(ArtistWithoutIdDTO? artist)
        {
            if (artist == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (await _authenticationService.EmailExistsAsync(artist.Email))
                throw new AppValidationException("Ez az e-mail cím már regisztrálva van.");
            return await CreateAsync(artist);
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

        public async Task<List<ArtistWithIdDTO>> ListBySalonForOwnerAsync(int ownerId, int salonId)
        {
            if (ownerId <= 0 || salonId <= 0)
                throw new AppValidationException("Érvénytelen tulajdonos vagy szalon azonosító.");

            var salon = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId);
            if (salon == null)
                throw new AppForbiddenException("Ehhez a szalonhoz nincs jogosultságod, vagy nem létezik.");

            var artists = await _artistRepository.ListBySalonIdAsync(salonId);
            return _mapper.Map<List<ArtistWithIdDTO>>(artists);
        }

        public async Task<List<ArtistWithIdDTO>> ListBySalonForPublicAsync(int salonId, int[]? mustOfferServiceIds = null)
        {
            if (salonId <= 0)
                return new List<ArtistWithIdDTO>();

            if (!await _salonRepository.ExistsByIdAsync(salonId))
                return new List<ArtistWithIdDTO>();

            var artists = await _artistRepository.ListBySalonIdAsync(salonId);
            if (mustOfferServiceIds != null && mustOfferServiceIds.Length > 0)
            {
                var eligible = await _offeringRepository.ListArtistIdsOfferingAllServicesInSalonAsync(salonId, mustOfferServiceIds);
                var ok = eligible.ToHashSet();
                artists = artists.Where(a => ok.Contains(a.Id)).ToList();
            }

            return _mapper.Map<List<ArtistWithIdDTO>>(artists);
        }

        public async Task<ArtistWithIdDTO?> GetByIdAsync(int id)
        {
            var artist = await _artistRepository.GetByIdAsync(id);
            return artist == null ? null : _mapper.Map<ArtistWithIdDTO>(artist);
        }

        public async Task<ArtistWithIdDTO> GetMeOrThrowAsync(int artistId)
        {
            var artist = await GetByIdAsync(artistId);
            return artist ?? throw new AppNotFoundException("A szakember profilja nem található.");
        }
    }
}
