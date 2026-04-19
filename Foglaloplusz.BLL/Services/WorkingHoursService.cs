using Foglaloplusz.DAL;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.Exceptions;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services;

public class WorkingHoursService : IWorkingHoursService
{
    private readonly IWorkingHoursRepository _workingHoursRepository;

    public WorkingHoursService(IWorkingHoursRepository workingHoursRepository)
    {
        _workingHoursRepository = workingHoursRepository;
    }

    public async Task Set(int artistId, WorkingHoursDTO? dto)
    {
        if (artistId <= 0)
            throw new AppValidationException("A szakember azonosítója érvénytelen.");
        if (dto == null)
            throw new AppValidationException("A kérés törzse kötelező.");

        var workDate = dto.Date.ToUtcDate();

        var existing = await _workingHoursRepository.GetByArtistAndDate(artistId, workDate);
        if (existing != null)
        {
            existing.Date = workDate;
            existing.StartTime = TimeSpan.Parse(dto.StartTime);
            existing.EndTime = TimeSpan.Parse(dto.EndTime);
            await _workingHoursRepository.Update(existing);
        }
        else
        {
            var workingHours = new WorkingHours
            {
                ArtistId = artistId,
                Date = workDate,
                StartTime = TimeSpan.Parse(dto.StartTime),
                EndTime = TimeSpan.Parse(dto.EndTime)
            };
            await _workingHoursRepository.Add(workingHours);
        }
    }

    public async Task SetSchedule(int artistId, SetWorkingHoursScheduleDTO? dto)
    {
        if (artistId <= 0)
            throw new AppValidationException("A szakember azonosítója érvénytelen.");
        if (dto == null)
            throw new AppValidationException("A kérés törzse kötelező.");
        if (dto.Days == null || dto.Days.Count == 0)
            throw new AppValidationException("Legalább egy nap megadása kötelező.");

        var rows = new List<WorkingHours>();
        for (var i = 0; i < dto.Days.Count; i++)
        {
            var day = dto.Days[i];
            var startRaw = string.IsNullOrWhiteSpace(day.Start) ? null : day.Start.Trim();
            var endRaw = string.IsNullOrWhiteSpace(day.End) ? null : day.End.Trim();
            var hasStart = startRaw != null;
            var hasEnd = endRaw != null;
            if (!hasStart && !hasEnd)
                continue;
            if (hasStart != hasEnd)
                throw new AppValidationException($"A(z) {i}. nap: add meg a kezdő és a záró időt is, vagy egyiket sem (zárva).");

            TimeSpan start;
            TimeSpan end;
            try
            {
                start = TimeSpan.Parse(startRaw!);
                end = TimeSpan.Parse(endRaw!);
            }
            catch (FormatException ex)
            {
                throw new AppValidationException($"A(z) {i}. nap: érvénytelen kezdő vagy záró idő.", ex);
            }

            if (start >= end)
                throw new AppValidationException($"A(z) {i}. nap: a kezdésnek a zárás előtt kell lennie.");

            var calendarDate = dto.StartDate.AddDays(i).ToDateTime(TimeOnly.MinValue).ToUtcDate();
            rows.Add(new WorkingHours
            {
                ArtistId = artistId,
                Date = calendarDate,
                StartTime = start,
                EndTime = end
            });
        }

        var rangeEnd = dto.StartDate.AddDays(dto.Days.Count - 1);
        await _workingHoursRepository.ReplaceForArtistDateRangeAsync(artistId, dto.StartDate, rangeEnd, rows);
    }

    public async Task<List<WorkingHoursDTO>> GetInRange(int artistId, WorkingHoursRangeRequestDTO? dto)
    {
        if (artistId <= 0)
            throw new AppValidationException("A szakember azonosítója érvénytelen.");
        if (dto == null)
            throw new AppValidationException("A kérés törzse kötelező.");
        if (dto.StartDate == default)
            throw new AppValidationException("A kezdő dátum kötelező.");
        if (dto.EndDate == default)
            throw new AppValidationException("A záró dátum kötelező.");
        if (dto.StartDate.Date > dto.EndDate.Date)
            throw new AppValidationException("A kezdő dátum nem lehet későbbi a záró dátumnál.");

        var rows = await _workingHoursRepository.GetByArtistAndDateRange(
            artistId,
            dto.StartDate,
            dto.EndDate);

        var rangeStart = dto.StartDate.ToUtcDate();
        var rangeEnd = dto.EndDate.ToUtcDate();

        var existingByDate = rows
            .GroupBy(w => w.Date.ToUtcDate())
            .ToDictionary(
                g => g.Key,
                g => g.First());

        var result = new List<WorkingHoursDTO>();
        for (var day = rangeStart; day <= rangeEnd; day = day.AddDays(1))
        {
            if (existingByDate.TryGetValue(day, out var row))
            {
                result.Add(new WorkingHoursDTO
                {
                    Date = day,
                    StartTime = row.StartTime.ToString(@"hh\:mm"),
                    EndTime = row.EndTime.ToString(@"hh\:mm")
                });
            }
            else
            {
                result.Add(new WorkingHoursDTO
                {
                    Date = day,
                    StartTime = "0",
                    EndTime = "0"
                });
            }
        }

        return result;
    }
}
