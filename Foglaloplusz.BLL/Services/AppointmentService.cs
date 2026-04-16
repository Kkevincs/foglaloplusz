using AutoMapper;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
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
        private readonly ISalonRepository _salonRepository;
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IWorkingHoursRepository workingHoursRepository,
            IServiceRepository serviceRepository,
            ISalonRepository salonRepository,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _workingHoursRepository = workingHoursRepository;
            _serviceRepository = serviceRepository;
            _salonRepository = salonRepository;
            _mapper = mapper;
        }

        public async Task<AppointmentsWithoutIdDTO> CreateAsync(AppointmentsWithoutIdDTO dto)
        {
            var serviceIds = dto.ServiceIds.Where(id => id > 0).ToList();
            if (serviceIds.Count == 0)
                throw new ArgumentException("Select at least one service.");

            var uniqueIds = serviceIds.Distinct().ToArray();
            var services = await _serviceRepository.GetByIdsAsync(uniqueIds);
            if (services.Length != uniqueIds.Length)
            {
                var foundIds = services.Select(s => s.Id).ToHashSet();
                var missing = uniqueIds.Where(id => !foundIds.Contains(id)).ToList();
                throw new ArgumentException(
                    $"Unknown service id(s): {string.Join(", ", missing)}.");
            }

            var byId = services.ToDictionary(s => s.Id);
            var orderedServices = serviceIds.Select(id => byId[id]).ToList();

            var serviceDuration = TimeSpan.FromMinutes(orderedServices.Sum(s => s.DurationInMinute));
            var endTime = dto.StartTime + serviceDuration + BlockBufferAfterServices;

            var workingHours = await _workingHoursRepository.GetByArtistAndDate(dto.ArtistId, dto.StartTime);
            if (workingHours == null)
                throw new ArgumentException("No working hours are set for the selected day.");

            var workStart = dto.StartTime.Date + workingHours.StartTime;
            var workEnd = dto.StartTime.Date + workingHours.EndTime;

            var isOverlapping = await _appointmentRepository.HasOverlapAsync(dto.ArtistId, dto.StartTime, endTime);

            if (dto.StartTime < workStart || endTime > workEnd)
                throw new ArgumentException("Appointment is outside of artist working hours.");

            if (isOverlapping)
                throw new ArgumentException("Appointment overlaps with an existing booking.");

            var appointment = new Appointment
            {
                GuestId = dto.GuestId,
                ArtistId = dto.ArtistId,
                SalonId = dto.SalonId > 0 ? dto.SalonId : null,
                StartTime = dto.StartTime,
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
                throw new ArgumentException(
                    $"Unknown service id(s): {string.Join(", ", missing)}.");
            }

            var byId = services.ToDictionary(s => s.Id);
            var orderedServices = orderedIds.Select(id => byId[id]).ToList();

            var serviceDuration = TimeSpan.FromMinutes(orderedServices.Sum(s => s.DurationInMinute));
            var totalBlockedDuration = serviceDuration + BlockBufferAfterServices;

            var workingHours = await _workingHoursRepository.GetByArtistAndDate(artistId, date);
            if (workingHours == null)
                return new List<AvaliableSlotDTO>();

            var existingAppointments = await _appointmentRepository.ListByArtistAndDateAsync(artistId, date);
            var availableSlots = new List<AvaliableSlotDTO>();
            var current = date.Date + workingHours.StartTime;
            var workEnd = date.Date + workingHours.EndTime;

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
                throw new ArgumentException("Artist id must be positive.", nameof(artistId));
            var apps = await _appointmentRepository.ListByArtistIdAsync(artistId);
            return _mapper.Map<List<AppointmentsWithIdDTO>>(apps);
        }

        public async Task<List<AppointmentsWithIdDTO>> ListForSalonOwnerAsync(int ownerId, int salonId)
        {
            var salon = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId);
            if (salon == null)
                throw new InvalidOperationException("Salon was not found.");

            var appointments = await _appointmentRepository.ListBySalonIdAsync(salonId);
            return _mapper.Map<List<AppointmentsWithIdDTO>>(appointments);
        }

        public Task<bool> CancelForGuestAsync(int guestId, int appointmentId) =>
            _appointmentRepository.DeleteForGuestAsync(guestId, appointmentId);
    }
}
