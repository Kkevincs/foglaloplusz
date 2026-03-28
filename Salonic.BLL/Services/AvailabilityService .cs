using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.Services.Interfaces;

public class AvailabilityService : IAvailabilityService
{
    private readonly IAppointmentService _appointmentService;

    public AvailabilityService(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<List<DayAvailabilityDto>> GetMonthAvailability(MonthAvailabilityRequestDto request)
    {
        if (request.Month < 1 || request.Month > 12)
            throw new ArgumentException("Invalid month");

        if (request.ServiceIds == null || request.ServiceIds.Length == 0)
            throw new ArgumentException("At least one service is required");

        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);

        var result = new List<DayAvailabilityDto>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(request.Year, request.Month, day);

            var slots = await _appointmentService
                .GetAvailableSlots(request.ArtistId, request.ServiceIds, date);

            result.Add(new DayAvailabilityDto
            {
                Date = date,
                HasAvailableAppointments = slots.Any()
            });
        }

        return result;
    }
}