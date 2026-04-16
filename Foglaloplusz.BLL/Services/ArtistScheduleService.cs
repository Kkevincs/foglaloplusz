using System.Globalization;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class ArtistScheduleService : IArtistScheduleService
    {
        private const int FirstHour = 8;
        private const int LastHour = 20;

        private readonly ISalonRepository _salonRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IWorkingHoursService _workingHoursService;
        private readonly IAppointmentRepository _appointmentRepository;

        public ArtistScheduleService(
            ISalonRepository salonRepository,
            IArtistRepository artistRepository,
            IWorkingHoursService workingHoursService,
            IAppointmentRepository appointmentRepository)
        {
            _salonRepository = salonRepository;
            _artistRepository = artistRepository;
            _workingHoursService = workingHoursService;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ArtistWeekScheduleDTO?> GetWeekForOwnerAsync(
            int ownerId,
            int salonId,
            int artistId,
            DateTime weekStartMonday,
            CancellationToken cancellationToken = default)
        {
            var salon = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId);
            if (salon == null)
                return null;

            var artist = await _artistRepository.GetByIdAsync(artistId);
            if (artist == null || artist.SalonId != salonId)
                return null;

            var monday = weekStartMonday.Date;
            var sunday = monday.AddDays(6);

            var hoursRows = await _workingHoursService.GetInRange(
                artistId,
                new WorkingHoursRangeRequestDTO
                {
                    StartDate = monday,
                    EndDate = sunday
                });

            var hoursByDate = hoursRows.ToDictionary(x => x.Date.Date, x => x);

            var rangeEndExclusive = monday.AddDays(7);
            var appointments = await _appointmentRepository.ListByArtistAndDateRangeAsync(
                artistId,
                monday,
                rangeEndExclusive);

            var dto = new ArtistWeekScheduleDTO
            {
                WeekStart = monday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ArtistId = artistId,
                Hours = Enumerable.Range(FirstHour, LastHour - FirstHour + 1).ToList()
            };

            for (var i = 0; i < 7; i++)
            {
                var dayDate = monday.AddDays(i);
                hoursByDate.TryGetValue(dayDate, out var whDay);

                var dayDto = new ArtistWeekScheduleDayDTO
                {
                    Date = dayDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Label = dayDate.ToString("ddd", CultureInfo.InvariantCulture),
                    Cells = new List<ArtistWeekScheduleCellDTO>()
                };

                TryGetOpenWindow(whDay, out var workStart, out var workEnd);

                var dayAppointments = FilterAppointmentsForCalendarDay(appointments, dayDate);

                for (var h = FirstHour; h <= LastHour; h++)
                {
                    var slotStart = dayDate + TimeSpan.FromHours(h);
                    var slotEnd = dayDate + TimeSpan.FromHours(h + 1);

                    var cell = new ArtistWeekScheduleCellDTO { Hour = h };

                    if (!SlotInWorkWindow(slotStart, slotEnd, workStart, workEnd))
                    {
                        cell.Status = "closed";
                    }
                    else if (TryFindOverlappingAppointment(dayAppointments, slotStart, slotEnd, out var appt))
                    {
                        cell.Status = "booked";
                        cell.Detail = string.IsNullOrWhiteSpace(appt!.Notes) ? "Booking" : appt.Notes.Trim();
                    }
                    else
                    {
                        cell.Status = "open";
                    }

                    dayDto.Cells.Add(cell);
                }

                dto.Days.Add(dayDto);
            }

            return dto;
        }

        private static List<Appointment> FilterAppointmentsForCalendarDay(IList<Appointment> all, DateTime calendarDay)
        {
            var dayStart = calendarDay.Date;
            var dayEnd = dayStart.AddDays(1);
            return all
                .Where(a => a.StartTime < dayEnd && a.EndTime > dayStart)
                .ToList();
        }

        private static bool TryFindOverlappingAppointment(
            List<Appointment> dayAppointments,
            DateTime slotStart,
            DateTime slotEnd,
            out Appointment appointment)
        {
            foreach (var a in dayAppointments)
            {
                if (slotStart < a.EndTime && slotEnd > a.StartTime)
                {
                    appointment = a;
                    return true;
                }
            }

            appointment = null!;
            return false;
        }

        private static bool SlotInWorkWindow(
            DateTime slotStart,
            DateTime slotEnd,
            TimeSpan workStart,
            TimeSpan workEnd)
        {
            var day = slotStart.Date;
            var windowStart = day + workStart;
            var windowEnd = day + workEnd;
            return slotStart < windowEnd && slotEnd > windowStart;
        }

        /// <summary>Closed day when missing row or same semantics as <see cref="WorkingHoursService.GetInRange"/> ("0"/"0").</summary>
        private static bool TryGetOpenWindow(WorkingHoursDTO? wh, out TimeSpan start, out TimeSpan end)
        {
            start = end = default;
            if (wh == null)
                return false;

            var s = wh.StartTime?.Trim() ?? string.Empty;
            var e = wh.EndTime?.Trim() ?? string.Empty;
            if (s == "0" && e == "0")
                return false;

            if (!TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out start))
                return false;
            if (!TimeSpan.TryParse(e, CultureInfo.InvariantCulture, out end))
                return false;
            if (start >= end)
                return false;
            return true;
        }
    }
}
