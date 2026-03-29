using AutoMapper;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic.BLL.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IMapper _mapper;

        public ServiceService(
            IServiceRepository repo,
            IArtistRepository artistRepository,
            IGuestRepository guestRepository,
            IMapper mapper)
        {
            _serviceRepository = repo;
            _artistRepository = artistRepository;
            _guestRepository = guestRepository;
            _mapper = mapper;
        }

        public async Task<List<ServiceWithIdDTO>> ListForSalonAsync(int salonId)
        {
            if (salonId <= 0)
                throw new ArgumentException("Salon id must be positive.", nameof(salonId));

            var list = await _serviceRepository.GetAllBySalonId(salonId);
            return _mapper.Map<List<ServiceWithIdDTO>>(list);
        }

        public async Task<BookingNamesResponseDTO> GetBookingDisplayNamesAsync(BookingNamesRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.ArtistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(request));
            if (request.GuestId <= 0)
                throw new ArgumentException("Guest id must be positive.", nameof(request));

            var artist = await _artistRepository.GetById(request.ArtistId);
            if (artist == null)
                throw new InvalidOperationException("Artist was not found.");

            if (artist.SalonId <= 0)
                throw new InvalidOperationException("Artist is not assigned to a salon.");

            var guest = await _guestRepository.GetById(request.GuestId);
            if (guest == null)
                throw new InvalidOperationException("Guest was not found.");

            var orderedIds = new List<int>();
            var seen = new HashSet<int>();
            foreach (var id in request.ServiceIds ?? Enumerable.Empty<int>())
            {
                if (id > 0 && seen.Add(id))
                    orderedIds.Add(id);
            }

            List<ServiceIdNameDTO> serviceDtos;
            if (orderedIds.Count == 0)
            {
                serviceDtos = new List<ServiceIdNameDTO>();
            }
            else
            {
                var rows = await _serviceRepository.GetIdAndNameBySalonAndArtistIds(
                    artist.SalonId,
                    request.ArtistId,
                    orderedIds.ToArray());
                var byId = rows.ToDictionary(r => r.Id, r => r.Name);
                serviceDtos = orderedIds
                    .Where(id => byId.ContainsKey(id))
                    .Select(id => new ServiceIdNameDTO { Id = id, Name = byId[id] })
                    .ToList();
            }

            return new BookingNamesResponseDTO
            {
                ArtistName = artist.Name ?? string.Empty,
                GuestName = guest.Name ?? string.Empty,
                Services = serviceDtos
            };
        }

        public async Task<ServiceWithIdDTO?> GetByIdForArtistAsync(int artistId, int salonId, int serviceId)
        {
            if (artistId <= 0 || salonId <= 0 || serviceId <= 0)
                return null;

            if (!await _serviceRepository.ArtistBelongsToSalon(artistId, salonId))
                return null;

            var service = await _serviceRepository.GetForArtist(artistId, salonId, serviceId);
            return service == null ? null : _mapper.Map<ServiceWithIdDTO>(service);
        }

        public async Task<ServiceWithIdDTO> CreateForArtistAsync(int artistId, int salonId, ServiceWithoutIdDTO dto)
        {
            if (!await _serviceRepository.ArtistBelongsToSalon(artistId, salonId))
                throw new InvalidOperationException("Salon was not found.");

            var entity = _mapper.Map<Service>(dto);
            entity.SalonId = salonId;
            var created = await _serviceRepository.CreateForArtist(artistId, entity);
            return _mapper.Map<ServiceWithIdDTO>(created);
        }

        public async Task UpdateForArtistAsync(int artistId, int salonId, ServiceWithIdDTO dto)
        {
            if (!await _serviceRepository.ArtistBelongsToSalon(artistId, salonId))
                throw new InvalidOperationException("Salon was not found.");

            var entity = _mapper.Map<Service>(dto);
            await _serviceRepository.UpdateForArtist(artistId, entity);
        }

        public async Task DeleteForArtistAsync(int artistId, int salonId, int serviceId)
        {
            if (!await _serviceRepository.ArtistBelongsToSalon(artistId, salonId))
                throw new InvalidOperationException("Salon was not found.");

            var deleted = await _serviceRepository.DeleteForArtistInSalon(artistId, salonId, serviceId);
            if (!deleted)
                throw new InvalidOperationException("Service was not found.");
        }

        private async Task<int> ResolveSalonIdForArtistOrThrowAsync(int artistId)
        {
            if (artistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(artistId));

            var artist = await _artistRepository.GetById(artistId);
            if (artist == null)
                throw new InvalidOperationException("Artist was not found.");

            if (artist.SalonId <= 0)
                throw new InvalidOperationException("Artist is not assigned to a salon.");

            return artist.SalonId;
        }

        public async Task<ServiceWithIdDTO> CreateForCurrentArtistAsync(int artistId, ServiceWithoutIdDTO dto)
        {
            var salonId = await ResolveSalonIdForArtistOrThrowAsync(artistId);
            return await CreateForArtistAsync(artistId, salonId, dto);
        }

        public async Task UpdateForCurrentArtistAsync(int artistId, ServiceWithIdDTO dto)
        {
            var salonId = await ResolveSalonIdForArtistOrThrowAsync(artistId);
            await UpdateForArtistAsync(artistId, salonId, dto);
        }

        public async Task DeleteForCurrentArtistAsync(int artistId, int serviceId)
        {
            var salonId = await ResolveSalonIdForArtistOrThrowAsync(artistId);
            await DeleteForArtistAsync(artistId, salonId, serviceId);
        }
    }
}
