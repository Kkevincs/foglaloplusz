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

        public async Task<ServiceWithIdDTO> CreateForCurrentArtistAsync(int artistId, ServiceWithoutIdDTO dto)
        {
            if (artistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(artistId));

            var artist = await _artistRepository.GetByIdAsync(artistId)
                ?? throw new InvalidOperationException("Artist was not found.");

            if (artist.SalonId <= 0)
                throw new InvalidOperationException("Artist is not assigned to a salon.");

            var entity = _mapper.Map<Service>(dto);
            entity.SalonId = artist.SalonId;
            entity.CreatedByArtistId = artistId;

            await _serviceRepository.AddAsync(entity);
            return _mapper.Map<ServiceWithIdDTO>(entity);
        }

        public async Task<BookingNamesResponseDTO> GetBookingDisplayNamesAsync(BookingNamesRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.ArtistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(request));
            if (request.GuestId <= 0)
                throw new ArgumentException("Guest id must be positive.", nameof(request));

            var artist = await _artistRepository.GetByIdAsync(request.ArtistId)
                ?? throw new InvalidOperationException("Artist was not found.");

            if (artist.SalonId <= 0)
                throw new InvalidOperationException("Artist is not assigned to a salon.");

            var guest = await _guestRepository.GetByIdAsync(request.GuestId)
                ?? throw new InvalidOperationException("Guest was not found.");

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
                var loaded = await _serviceRepository.GetByIdsAsync(orderedIds.ToArray());
                var filtered = loaded
                    .Where(s =>
                        s.SalonId == artist.SalonId &&
                        s.CreatedByArtistId == request.ArtistId)
                    .ToDictionary(s => s.Id, s => s.Name ?? string.Empty);

                serviceDtos = orderedIds
                    .Where(id => filtered.ContainsKey(id))
                    .Select(id => new ServiceIdNameDTO { Id = id, Name = filtered[id] })
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

            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null ||
                service.SalonId != salonId ||
                service.CreatedByArtistId != artistId)
                return null;

            return _mapper.Map<ServiceWithIdDTO>(service);
        }

        public async Task<List<ServiceWithIdDTO>> ListForCurrentArtistAsync(int artistId)
        {
            if (artistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(artistId));

            var list = await _serviceRepository.ListByArtistIdAsync(artistId);
            return _mapper.Map<List<ServiceWithIdDTO>>(list);
        }

        public async Task<List<ServiceWithIdDTO>> ListForSalonAsync(int salonId)
        {
            if (salonId <= 0)
                throw new ArgumentException("Salon id must be positive.", nameof(salonId));

            var list = await _serviceRepository.ListBySalonIdAsync(salonId);
            return _mapper.Map<List<ServiceWithIdDTO>>(list);
        }
    }
}
