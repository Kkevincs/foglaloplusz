using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Exceptions;
using Foglaloplusz.BLL.Services.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IAppointmentService _appointmentService;

        public AvailabilityService(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public async Task<List<DayAvailabilityDto>> GetMonthAvailability(MonthAvailabilityRequestDto? request)
        {
            if (request == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (request.Month < 1 || request.Month > 12)
                throw new AppValidationException("A hónap 1 és 12 között lehet.");
            if (request.Year < 1 || request.Year > 9999)
                throw new AppValidationException("Az év érvénytelen.");
            if (request.ArtistId <= 0)
                throw new AppValidationException("A szakember azonosítója érvénytelen.");

            if (request.ServiceIds == null || request.ServiceIds.Length == 0)
                throw new AppValidationException("Legalább egy szolgáltatást meg kell adni.");

            var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
            var result = new List<DayAvailabilityDto>();

            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = DateTime.SpecifyKind(new DateTime(request.Year, request.Month, day), DateTimeKind.Utc);
                var slots = await _appointmentService.GetAvailableSlotsAsync(
                    request.ArtistId,
                    request.ServiceIds,
                    date);

                result.Add(new DayAvailabilityDto
                {
                    Date = date,
                    HasAvailableAppointments = slots.Any()
                });
            }

            return result;
        }
    }
}
