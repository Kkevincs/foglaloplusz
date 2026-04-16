using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.DTOs.ServiceDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
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

        public async Task<ServiceCsvImportResponseDTO> ImportServicesFromCsvAsync(int artistId, string csv)
        {
            if (artistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(artistId));
            if (string.IsNullOrWhiteSpace(csv))
                throw new ArgumentException("CSV text is required.");

            const int maxChars = 512_000;
            if (csv.Length > maxChars)
                throw new ArgumentException("CSV file is too large.");

            var result = new ServiceCsvImportResponseDTO();
            var rawLines = csv.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var lines = new List<(int LineNumber, string Content)>();
            var displayLine = 0;
            foreach (var raw in rawLines)
            {
                displayLine++;
                var trimmed = raw.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                lines.Add((displayLine, trimmed));
            }

            if (lines.Count == 0)
                throw new ArgumentException("CSV contains no data rows.");

            const int maxRows = 300;
            if (lines.Count > maxRows)
                throw new ArgumentException($"CSV may contain at most {maxRows} non-empty lines.");

            var startIndex = 0;
            if (lines.Count > 0 && LooksLikeCsvHeaderLine(lines[0].Content))
                startIndex = 1;

            if (startIndex >= lines.Count)
                throw new ArgumentException("CSV contains only a header row.");

            for (var i = startIndex; i < lines.Count; i++)
            {
                var (lineNumber, content) = lines[i];
                try
                {
                    var dto = ParseServiceCsvLine(content);
                    var created = await CreateForCurrentArtistAsync(artistId, dto);
                    result.Created.Add(created);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ServiceCsvImportErrorDTO
                    {
                        Line = lineNumber,
                        Message = ex.Message,
                    });
                }
            }

            return result;
        }

        private static bool LooksLikeCsvHeaderLine(string line)
        {
            var p = line.Split(';');
            if (p.Length < 4) return false;
            return p[0].Trim().Equals("servicename", StringComparison.OrdinalIgnoreCase)
                && p[1].Trim().Equals("duration", StringComparison.OrdinalIgnoreCase);
        }

        private static ServiceWithoutIdDTO ParseServiceCsvLine(string line)
        {
            var parts = line.Split(';');
            if (parts.Length < 4)
                throw new InvalidOperationException(
                    "Each line needs servicename;duration;description;price (empty description: ;; ).");

            var name = parts[0].Trim();
            var durationStr = parts[1].Trim();
            string description;
            string priceStr;

            if (parts.Length == 4)
            {
                description = parts[2].Trim();
                priceStr = parts[3].Trim();
            }
            else
            {
                description = string.Join(";", parts.Skip(2).Take(parts.Length - 3)).Trim();
                priceStr = parts[parts.Length - 1].Trim();
            }

            if (string.IsNullOrEmpty(name))
                throw new InvalidOperationException("Service name is required.");

            if (!int.TryParse(durationStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var duration)
                || duration < 1)
                throw new InvalidOperationException("Duration must be a positive integer (minutes).");

            var price = 0;
            if (!int.TryParse(priceStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out price))
            {
                if (decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
                    price = (int)Math.Round(dec, MidpointRounding.AwayFromZero);
                else
                    throw new InvalidOperationException("Price must be a number.");
            }

            if (price < 0)
                throw new InvalidOperationException("Price must be non-negative.");

            return new ServiceWithoutIdDTO
            {
                Name = name,
                Description = description ?? string.Empty,
                DurationInMinute = duration,
                Price = price,
            };
        }
    }
}
