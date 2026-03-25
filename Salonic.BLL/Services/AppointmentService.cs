using AutoMapper;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salonic.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private static readonly TimeSpan BlockBufferAfterServices = TimeSpan.FromMinutes(15);

        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IWorkingHoursRepository _workingHoursRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IWorkingHoursRepository workingHoursRepository,
            IServiceRepository serviceRepository,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _workingHoursRepository = workingHoursRepository;
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<AppointmentsWithoutIdDTO> Create(AppointmentsWithoutIdDTO dto)
        {
            if (dto.SalonId <= 0)
                throw new InvalidOperationException("SalonId is required.");

            var serviceIds = dto.ServiceIds?
                .Where(id => id > 0)
                .ToList() ?? new List<int>();

            if (serviceIds.Count == 0)
                throw new InvalidOperationException("At least one service is required.");

            var uniqueIds = serviceIds.Distinct().ToArray();
            var services = await _serviceRepository.GetByIds(uniqueIds);
            if (services.Length != uniqueIds.Length)
                throw new InvalidOperationException("One or more services were not found.");

            var byId = services.ToDictionary(s => s.Id);
            var orderedServices = serviceIds.Select(id => byId[id]).ToList();

            var serviceDuration = TimeSpan.FromMinutes(orderedServices.Sum(s => s.DurationInMinute));
            var endTime = dto.StartTime + serviceDuration + BlockBufferAfterServices;

            var workingHours = await _workingHoursRepository
                .GetByArtistAndDate(dto.ArtistId, dto.StartTime);

            if (workingHours == null)
                throw new InvalidOperationException("No working hours set for this date.");

            var workStart = dto.StartTime.Date + workingHours.StartTime;
            var workEnd = dto.StartTime.Date + workingHours.EndTime;

            if (dto.StartTime < workStart || endTime > workEnd)
                throw new InvalidOperationException("Appointment is outside working hours.");

            var isOverlapping = await _appointmentRepository
                .IsOverlapping(dto.ArtistId, dto.StartTime, endTime);

            if (isOverlapping)
                throw new InvalidOperationException("This slot is no longer available.");

            var appointment = new Appointment
            {
                GuestId = dto.GuestId,
                ArtistId = dto.ArtistId,
                SalonId = dto.SalonId,
                StartTime = dto.StartTime,
                EndTime = endTime,
                Notes = dto.Notes,
                Services = orderedServices.ToList()
            };

            await _appointmentRepository.Create(appointment);
            return dto;
        }


        public async Task DeleteById(int id)
        {
            await _appointmentRepository.DeleteById(id);
        }

        public async Task<List<AppointmentsWithIdDTO>> GetAll()
        {
            var appointments = await _appointmentRepository.GetAll();
            return _mapper.Map<List<AppointmentsWithIdDTO>>(appointments);
        }

        public async Task<List<AvaliableSlotDTO>> GetAvailableSlots(int artistId, int[] serviceIds, DateTime date)
        {
            if (serviceIds == null || serviceIds.Length == 0)
                return new List<AvaliableSlotDTO>();

            var orderedIds = serviceIds.Where(id => id > 0).ToList();
            if (orderedIds.Count == 0)
                return new List<AvaliableSlotDTO>();

            var uniqueIds = orderedIds.Distinct().ToArray();
            var services = await _serviceRepository.GetByIds(uniqueIds);
            if (services.Length != uniqueIds.Length)
                return new List<AvaliableSlotDTO>();

            var byId = services.ToDictionary(s => s.Id);
            var orderedServices = orderedIds.Select(id => byId[id]).ToList();

            var serviceDuration = TimeSpan.FromMinutes(orderedServices.Sum(s => s.DurationInMinute));
            var totalBlockedDuration = serviceDuration + BlockBufferAfterServices;

            var workingHours = await _workingHoursRepository.GetByArtistAndDate(artistId, date);
            if (workingHours == null) return new List<AvaliableSlotDTO>();

            var existingAppointments = await _appointmentRepository.GetByArtistAndDate(artistId, date);

            var availableSlots = new List<AvaliableSlotDTO>();
            var current = date.Date + workingHours.StartTime;
            var workEnd = date.Date + workingHours.EndTime;

            while (current + totalBlockedDuration <= workEnd)
            {
                var slotEnd = current + totalBlockedDuration;

                var isOverlapping = existingAppointments.Any(a =>
                    current < a.EndTime && slotEnd > a.StartTime);

                if (!isOverlapping)
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

        public async Task<List<AppointmentsWithIdDTO>> GetByArtistId(int id)
        {
            var app = await _appointmentRepository.GetByArtistId(id);
            return _mapper.Map<List<AppointmentsWithIdDTO>>(app);
        }

        public async Task<List<AppointmentsWithIdDTO>> GetByGuestId(int id)
        {
            var app = await _appointmentRepository.GetByGuestId(id);
            return _mapper.Map<List<AppointmentsWithIdDTO>>(app);
        }

        public async Task<AppointmentsWithIdDTO> GetById(int id)
        {
            var appointment = await _appointmentRepository.GetById(id);
            return _mapper.Map<AppointmentsWithIdDTO>(appointment);
        }

        public async Task Update(AppointmentsWithIdDTO app)
        {
            var appointment = _mapper.Map<Appointment>(app);
            await _appointmentRepository.Update(appointment);
        }
    }
}
