using AutoMapper;
using Foglaloplusz.DAL;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Exceptions;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private static readonly TimeSpan BlockBufferAfterServices = TimeSpan.FromMinutes(15);

        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IWorkingHoursRepository _workingHoursRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IOfferingRepository _offeringRepository;
        private readonly ISalonRepository _salonRepository;
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IWorkingHoursRepository workingHoursRepository,
            IServiceRepository serviceRepository,
            IOfferingRepository offeringRepository,
            ISalonRepository salonRepository,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _workingHoursRepository = workingHoursRepository;
            _serviceRepository = serviceRepository;
            _offeringRepository = offeringRepository;
            _salonRepository = salonRepository;
            _mapper = mapper;
        }

        public async Task<List<AvaliableSlotDTO>> GetAvailableSlotsFromRequestAsync(GetAvailableSlotsDTO? request)
        {
            if (request == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            return await GetAvailableSlotsAsync(
                request.ArtistId,
                request.ServiceIds?.ToArray() ?? Array.Empty<int>(),
                request.Date);
        }

        public async Task<AppointmentsWithoutIdDTO> CreateAsync(AppointmentsWithoutIdDTO? dto)
        {
            if (dto == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            var serviceIds = dto.ServiceIds.Where(id => id > 0).ToList();
            if (serviceIds.Count == 0)
                throw new AppValidationException("Legalább egy szolgáltatást válassz.");

            var uniqueIds = serviceIds.Distinct().ToArray();
            var services = await _serviceRepository.GetByIdsAsync(uniqueIds);
            if (services.Length != uniqueIds.Length)
            {
                var foundIds = services.Select(s => s.Id).ToHashSet();
                var missing = uniqueIds.Where(id => !foundIds.Contains(id)).ToList();
                throw new AppValidationException(
                    $"Ismeretlen szolgáltatás-azonosító: {string.Join(", ", missing)}.");
            }

            var byId = services.ToDictionary(s => s.Id);
            var orderedServices = serviceIds.Select(id => byId[id]).ToList();

            if (!await _offeringRepository.ArtistOffersAllServicesAsync(dto.ArtistId, uniqueIds))
                throw new AppValidationException("A szakember nem kínálja a megadott szolgáltatások mindegyikét.");

            var serviceDuration = TimeSpan.FromMinutes(orderedServices.Sum(s => s.DurationInMinute));
            var startUtc = dto.StartTime.ToUtcInstant();
            var endTime = startUtc + serviceDuration + BlockBufferAfterServices;

            var workingHours = await _workingHoursRepository.GetByArtistAndDate(dto.ArtistId, startUtc);
            if (workingHours == null)
                throw new AppValidationException("A kiválasztott napra nincs beállítva munkaidő.");

            var workStart = startUtc.Date + workingHours.StartTime;
            var workEnd = startUtc.Date + workingHours.EndTime;

            var isOverlapping = await _appointmentRepository.HasOverlapAsync(dto.ArtistId, startUtc, endTime);

            if (startUtc < workStart || endTime > workEnd)
                throw new AppValidationException("Az időpont a munkaidőn kívül esik.");

            if (isOverlapping)
                throw new AppValidationException("Az időpont ütközik egy meglévő foglalással.");

            var appointment = new Appointment
            {
                GuestId = dto.GuestId,
                ArtistId = dto.ArtistId,
                SalonId = dto.SalonId > 0 ? dto.SalonId : null,
                StartTime = startUtc,
                EndTime = endTime,
                Notes = dto.Notes ?? string.Empty,
                Services = orderedServices.ToList()
            };

            await _appointmentRepository.AddAsync(appointment);
            return dto;
        }

        public async Task<List<AvaliableSlotDTO>> GetAvailableSlotsAsync(int artistId, int[] serviceIds, DateTime date)
        {
            var orderedIds = serviceIds.Where(id => id > 0).ToList();
            if (orderedIds.Count == 0)
                return new List<AvaliableSlotDTO>();

            var uniqueIds = orderedIds.Distinct().ToArray();
            var services = await _serviceRepository.GetByIdsAsync(uniqueIds);
            if (services.Length != uniqueIds.Length)
            {
                var foundIds = services.Select(s => s.Id).ToHashSet();
                var missing = uniqueIds.Where(id => !foundIds.Contains(id)).ToList();
                throw new AppValidationException(
                    $"Ismeretlen szolgáltatás-azonosító: {string.Join(", ", missing)}.");
            }

            var byId = services.ToDictionary(s => s.Id);
            var orderedServices = orderedIds.Select(id => byId[id]).ToList();

            if (!await _offeringRepository.ArtistOffersAllServicesAsync(artistId, uniqueIds))
                throw new AppValidationException("A szakember nem kínálja a megadott szolgáltatások mindegyikét.");

            var serviceDuration = TimeSpan.FromMinutes(orderedServices.Sum(s => s.DurationInMinute));
            var totalBlockedDuration = serviceDuration + BlockBufferAfterServices;

            var workingHours = await _workingHoursRepository.GetByArtistAndDate(artistId, date);
            if (workingHours == null)
                return new List<AvaliableSlotDTO>();

            var existingAppointments = await _appointmentRepository.ListByArtistAndDateAsync(artistId, date);
            var availableSlots = new List<AvaliableSlotDTO>();
            var dayStart = date.ToUtcDate();
            var current = dayStart + workingHours.StartTime;
            var workEnd = dayStart + workingHours.EndTime;

            while (current + totalBlockedDuration <= workEnd)
            {
                var slotEnd = current + totalBlockedDuration;

                var overlaps = existingAppointments.Any(a =>
                    current < a.EndTime && slotEnd > a.StartTime);

                if (!overlaps)
                {
                    availableSlots.Add(new AvaliableSlotDTO
                    {
                        StartTime = current,
                        EndTime = current + serviceDuration
                    });
                }

                current = current + TimeSpan.FromMinutes(15);
            }

            return availableSlots;
        }

        public async Task<List<AppointmentsWithIdDTO>> ListForGuestAsync(int guestId)
        {
            var apps = await _appointmentRepository.ListByGuestIdAsync(guestId);
            return _mapper.Map<List<AppointmentsWithIdDTO>>(apps);
        }

        public async Task<List<AppointmentsWithIdDTO>> ListForArtistAsync(int artistId)
        {
            if (artistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");
            var apps = await _appointmentRepository.ListByArtistIdAsync(artistId);
            return _mapper.Map<List<AppointmentsWithIdDTO>>(apps);
        }

        public async Task<List<AppointmentsWithIdDTO>> ListForSalonOwnerAsync(int ownerId, int salonId)
        {
            var salon = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId);
            if (salon == null)
                throw new AppForbiddenException("Ehhez a szalonhoz nincs jogosultságod, vagy nem létezik.");

            var appointments = await _appointmentRepository.ListBySalonIdAsync(salonId);
            return _mapper.Map<List<AppointmentsWithIdDTO>>(appointments);
        }

        public Task<bool> CancelForGuestAsync(int guestId, int appointmentId) =>
            _appointmentRepository.DeleteForGuestAsync(guestId, appointmentId);
    }
}
