using System.Globalization;
using System.Linq;
using AutoMapper;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.DTOs.ServiceDTOs;
using Foglaloplusz.BLL.Exceptions;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IOfferingRepository _offeringRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly ISalonRepository _salonRepository;
        private readonly IMapper _mapper;

        public ServiceService(
            IServiceRepository repo,
            IOfferingRepository offeringRepository,
            IArtistRepository artistRepository,
            IGuestRepository guestRepository,
            ISalonRepository salonRepository,
            IMapper mapper)
        {
            _serviceRepository = repo;
            _offeringRepository = offeringRepository;
            _artistRepository = artistRepository;
            _guestRepository = guestRepository;
            _salonRepository = salonRepository;
            _mapper = mapper;
        }

        public async Task<ServiceWithIdDTO> CreateForCurrentArtistAsync(int artistId, ServiceWithoutIdDTO? dto)
        {
            if (dto == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (artistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");

            var artist = await _artistRepository.GetByIdAsync(artistId)
                ?? throw new AppValidationException("A szakember nem található.");

            if (artist.SalonId <= 0)
                throw new AppValidationException("A szakember nincs szalonhoz rendelve.");

            var entity = _mapper.Map<Service>(dto);
            entity.SalonId = artist.SalonId;
            entity.ArtistId = artistId;
            entity.OwnerId = null;

            await _serviceRepository.AddAsync(entity);

            await _offeringRepository.AddAsync(new Offering
            {
                ServiceId = entity.Id,
                ArtistId = artistId,
                IsImported = false,
            });

            return MapServiceRow(entity);
        }

        public async Task<ServiceWithIdDTO> CreateForSalonOwnerAsync(int ownerId, int salonId, ServiceWithoutIdDTO? dto)
        {
            if (dto == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            var salon = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId)
                ?? throw new AppValidationException("A szalon nem található, vagy nincs jogosultságod.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppValidationException("A megnevezés kötelező.");
            if (dto.DurationInMinute < 1)
                throw new AppValidationException("Az időtartam legalább 1 perc legyen.");
            if (dto.Price < 0)
                throw new AppValidationException("Az ár nem lehet negatív.");

            var entity = _mapper.Map<Service>(dto);
            entity.SalonId = salonId;
            entity.OwnerId = ownerId;
            entity.ArtistId = null;

            await _serviceRepository.AddAsync(entity);
            return MapServiceRow(entity);
        }

        public async Task<ServiceWithIdDTO> UpdateForSalonOwnerAsync(int ownerId, int salonId, int serviceId, ServiceWithoutIdDTO? dto)
        {
            if (dto == null)
                throw new AppValidationException("A kérés törzse kötelező.");

            var existing = await _serviceRepository.GetByIdAsync(serviceId)
                ?? throw new AppValidationException("A szolgáltatás nem található.");

            if (existing.SalonId != salonId || existing.OwnerId != ownerId)
                throw new AppForbiddenException("Nincs jogosultságod ehhez a szolgáltatáshoz.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppValidationException("A megnevezés kötelező.");
            if (dto.DurationInMinute < 1)
                throw new AppValidationException("Az időtartam legalább 1 perc legyen.");
            if (dto.Price < 0)
                throw new AppValidationException("Az ár nem lehet negatív.");

            existing.Name = dto.Name.Trim();
            existing.Description = (dto.Description ?? string.Empty).Trim();
            existing.Price = dto.Price;
            existing.DurationInMinute = dto.DurationInMinute;

            await _serviceRepository.UpdateAsync(existing);
            return MapServiceRow(existing);
        }

        public async Task<bool> DeleteForSalonOwnerAsync(int ownerId, int salonId, int serviceId)
        {
            var existing = await _serviceRepository.GetByIdAsync(serviceId);
            if (existing == null || existing.SalonId != salonId || existing.OwnerId != ownerId)
                return false;

            return await _serviceRepository.DeleteAsync(serviceId);
        }

        public async Task<bool> DeleteForCurrentArtistAsync(int artistId, int serviceId)
        {
            if (artistId <= 0 || serviceId <= 0)
                return false;

            var existing = await _serviceRepository.GetByIdAsync(serviceId);
            if (existing == null)
                return false;
            if (existing.OwnerId != null)
                return false;
            if (existing.ArtistId != artistId)
                return false;

            return await _serviceRepository.DeleteAsync(serviceId);
        }

        public async Task<ServiceWithIdDTO> LinkSalonServiceForArtistAsync(int artistId, int serviceId)
        {
            var artist = await _artistRepository.GetByIdAsync(artistId)
                ?? throw new AppValidationException("A szakember nem található.");

            var service = await _serviceRepository.GetByIdAsync(serviceId)
                ?? throw new AppValidationException("A szolgáltatás nem található.");

            if (service.SalonId != artist.SalonId)
                throw new AppValidationException("A szolgáltatás nem ehhez a szalonhoz tartozik.");

            if (service.OwnerId == null)
                throw new AppValidationException("Csak a tulaj által létrehozott katalógus-szolgáltatás vehető fel.");

            if (await _offeringRepository.ExistsAsync(artistId, serviceId))
                throw new AppValidationException("Ez a szolgáltatás már szerepel a listádon.");

            await _offeringRepository.AddAsync(new Offering
            {
                ServiceId = serviceId,
                ArtistId = artistId,
                IsImported = true,
            });

            var o = await _offeringRepository.GetByArtistAndServiceAsync(artistId, serviceId);
            var dto = MapServiceRow(service);
            if (o != null)
            {
                dto.OfferingId = o.Id;
                dto.IsImported = o.IsImported;
            }
            return dto;
        }

        public async Task<bool> UnlinkOfferingForArtistAsync(int artistId, int offeringId)
        {
            var o = await _offeringRepository.GetByIdAsync(offeringId);
            if (o == null || o.ArtistId != artistId)
                return false;
            if (!o.IsImported)
                throw new AppValidationException("A saját szolgáltatás törlése más művelettel lehetséges.");
            return await _offeringRepository.DeleteAsync(offeringId);
        }

        public async Task<ServiceWithIdDTO> UpdateForCurrentArtistAsync(int artistId, int serviceId, ServiceWithoutIdDTO? dto)
        {
            if (dto == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (artistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");
            if (serviceId <= 0)
                throw new AppValidationException("A szolgáltatás azonosítója érvénytelen.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppValidationException("A megnevezés kötelező.");
            if (dto.DurationInMinute < 1)
                throw new AppValidationException("Az időtartam legalább 1 perc legyen.");
            if (dto.Price < 0)
                throw new AppValidationException("Az ár nem lehet negatív.");

            var existing = await _serviceRepository.GetByIdAsync(serviceId)
                ?? throw new AppNotFoundException("A szolgáltatás nem található.");

            if (existing.OwnerId != null)
                throw new AppForbiddenException("Csak a saját létrehozású szolgáltatás szerkeszthető.");

            if (existing.ArtistId != artistId)
                throw new AppForbiddenException("Nincs jogosultságod ehhez a szolgáltatáshoz.");

            existing.Name = dto.Name.Trim();
            existing.Description = (dto.Description ?? string.Empty).Trim();
            existing.Price = dto.Price;
            existing.DurationInMinute = dto.DurationInMinute;

            await _serviceRepository.UpdateAsync(existing);
            return MapServiceRow(existing);
        }

        public async Task<BookingNamesResponseDTO> GetBookingDisplayNamesAsync(BookingNamesRequestDTO? request)
        {
            if (request == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (request.ArtistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");
            if (request.GuestId <= 0)
                throw new AppValidationException("A vendég azonosítója érvénytelen.");

            var artist = await _artistRepository.GetByIdAsync(request.ArtistId)
                ?? throw new AppValidationException("A szakember nem található.");

            if (artist.SalonId <= 0)
                throw new AppValidationException("A szakember nincs szalonhoz rendelve.");

            var guest = await _guestRepository.GetByIdAsync(request.GuestId)
                ?? throw new AppValidationException("A vendég nem található.");

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
                if (!await _offeringRepository.ArtistOffersAllServicesAsync(request.ArtistId, orderedIds))
                    throw new AppValidationException("A szakember nem kínálja a megadott szolgáltatások mindegyikét.");

                var loaded = await _serviceRepository.GetByIdsAsync(orderedIds.ToArray());
                var filtered = loaded
                    .Where(s => s.SalonId == artist.SalonId)
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

            var o = await _offeringRepository.GetByArtistAndServiceAsync(artistId, serviceId);
            if (o?.Service == null || o.Service.SalonId != salonId)
                return null;

            var dto = MapServiceRow(o.Service);
            dto.OfferingId = o.Id;
            dto.IsImported = o.IsImported;
            return dto;
        }

        public async Task<List<ServiceWithIdDTO>> ListForCurrentArtistAsync(int artistId)
        {
            if (artistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");

            var offerings = await _offeringRepository.ListByArtistWithServiceAsync(artistId);
            return offerings.Select(o =>
            {
                var dto = MapServiceRow(o.Service);
                dto.OfferingId = o.Id;
                dto.IsImported = o.IsImported;
                return dto;
            }).ToList();
        }

        public async Task<List<ServiceWithIdDTO>> ListForSalonAsync(int salonId)
        {
            if (salonId <= 0)
                throw new AppValidationException("A szalon azonosítója érvénytelen.");

            var list = await _serviceRepository.ListBySalonIdWithOfferingsAsync(salonId);
            return list.Select(MapServiceRow).ToList();
        }

        public async Task<List<ServiceWithIdDTO>> ListCatalogForSalonOwnerAsync(int ownerId, int salonId)
        {
            if (ownerId <= 0)
                throw new AppValidationException("A tulajdonos azonosítója érvénytelen.");
            if (salonId <= 0)
                throw new AppValidationException("A szalon azonosítója érvénytelen.");

            _ = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId)
                ?? throw new AppValidationException("A szalon nem található, vagy nincs jogosultságod.");

            var list = await _serviceRepository.ListBySalonIdAsync(salonId);
            return list.Select(MapServiceRow).ToList();
        }

        public async Task<List<ServiceWithIdDTO>> ListCatalogForSalonArtistAsync(int artistId, int salonId)
        {
            if (artistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");
            if (salonId <= 0)
                throw new AppValidationException("A szalon azonosítója érvénytelen.");

            var artist = await _artistRepository.GetByIdAsync(artistId)
                ?? throw new AppValidationException("A szakember nem található.");
            if (artist.SalonId != salonId)
                throw new AppForbiddenException("A szakember nem ehhez a szalonhoz tartozik.");

            var list = await _serviceRepository.ListBySalonIdAsync(salonId);
            return list.Select(MapServiceRow).ToList();
        }

        public async Task<List<ServiceWithIdDTO>> ListOfferedForArtistInSalonAsync(int salonId, int artistId)
        {
            if (salonId <= 0)
                throw new AppValidationException("A szalon azonosítója érvénytelen.");
            if (artistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");

            var offerings = await _offeringRepository.ListByArtistWithServiceAsync(artistId);
            return offerings
                .Where(o => o.Service.SalonId == salonId)
                .Select(o =>
                {
                    var dto = MapServiceRow(o.Service);
                    dto.OfferingId = o.Id;
                    dto.IsImported = o.IsImported;
                    return dto;
                })
                .ToList();
        }

        public async Task<ServiceCsvImportResponseDTO> ImportServicesFromCsvAsync(int artistId, string csv)
        {
            if (artistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");
            if (string.IsNullOrWhiteSpace(csv))
                throw new AppValidationException("CSV szöveg kötelező.");

            const int maxChars = 512_000;
            if (csv.Length > maxChars)
                throw new AppValidationException("A CSV fájl túl nagy.");

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
                throw new AppValidationException("A CSV nem tartalmaz adatsorokat.");

            const int maxRows = 300;
            if (lines.Count > maxRows)
                throw new AppValidationException($"Legfeljebb {maxRows} nem üres sor engedélyezett.");

            var startIndex = 0;
            if (lines.Count > 0 && LooksLikeCsvHeaderLine(lines[0].Content))
                startIndex = 1;

            if (startIndex >= lines.Count)
                throw new AppValidationException("A CSV csak fejlécet tartalmaz.");

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

        public async Task<ServiceCsvImportResponseDTO> ImportSalonOwnerServicesFromCsvAsync(int ownerId, int salonId, string csv)
        {
            if (ownerId <= 0)
                throw new AppValidationException("A tulajdonos azonosítója érvénytelen.");
            if (salonId <= 0)
                throw new AppValidationException("A szalon azonosítója érvénytelen.");
            if (string.IsNullOrWhiteSpace(csv))
                throw new AppValidationException("CSV szöveg kötelező.");

            _ = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId)
                ?? throw new AppValidationException("A szalon nem található, vagy nincs jogosultságod.");

            const int maxChars = 512_000;
            if (csv.Length > maxChars)
                throw new AppValidationException("A CSV fájl túl nagy.");

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
                throw new AppValidationException("A CSV nem tartalmaz adatsorokat.");

            const int maxRows = 300;
            if (lines.Count > maxRows)
                throw new AppValidationException($"Legfeljebb {maxRows} nem üres sor engedélyezett.");

            var startIndex = 0;
            if (lines.Count > 0 && LooksLikeCsvHeaderLine(lines[0].Content))
                startIndex = 1;

            if (startIndex >= lines.Count)
                throw new AppValidationException("A CSV csak fejlécet tartalmaz.");

            for (var i = startIndex; i < lines.Count; i++)
            {
                var (lineNumber, content) = lines[i];
                try
                {
                    var dto = ParseServiceCsvLine(content);
                    var created = await CreateForSalonOwnerAsync(ownerId, salonId, dto);
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

        private static ServiceWithIdDTO MapServiceRow(Service s) =>
            new()
            {
                Id = s.Id,
                Name = s.Name ?? string.Empty,
                Description = s.Description ?? string.Empty,
                Price = s.Price,
                DurationInMinute = s.DurationInMinute,
                SalonId = s.SalonId,
                OwnerId = s.OwnerId,
                OwnerArtistId = s.ArtistId,
            };

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
                throw new AppValidationException(
                    "Minden sor formátuma: megnevezés;időtartam;leírás;ár (üres leírás: ;; ).");

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
                throw new AppValidationException("A szolgáltatás neve kötelező.");

            if (!int.TryParse(durationStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var duration)
                || duration < 1)
                throw new AppValidationException("Az időtartam pozitív egész szám kell legyen (perc).");

            var price = 0;
            if (!int.TryParse(priceStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out price))
            {
                if (decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
                    price = (int)Math.Round(dec, MidpointRounding.AwayFromZero);
                else
                    throw new AppValidationException("Az ár szám kell legyen.");
            }

            if (price < 0)
                throw new AppValidationException("Az ár nem lehet negatív.");

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
