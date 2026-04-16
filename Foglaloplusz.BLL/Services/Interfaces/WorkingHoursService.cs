using AutoMapper;
using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public class WorkingHoursService : IWorkingHoursService
    {
        private readonly IWorkingHoursRepository _workingHoursRepository;
        private readonly IMapper _mapper;

        public WorkingHoursService(IWorkingHoursRepository workingHoursRepository, IMapper mapper)
        {
            _workingHoursRepository = workingHoursRepository;
            _mapper = mapper;
        }

        public async Task Set(int artistId, WorkingHoursDTO dto)
        {
            if (artistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(artistId));

            var workDate = dto.Date.Date;

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

        public async Task SetSchedule(int artistId, SetWorkingHoursScheduleDTO dto)
        {
            if (artistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(artistId));
            if (dto.Days == null || dto.Days.Count == 0)
                throw new ArgumentException("Days must contain at least one entry.", nameof(dto));

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
                    throw new ArgumentException($"Day at index {i}: set both Start and End, or neither (closed).");

                TimeSpan start;
                TimeSpan end;
                try
                {
                    start = TimeSpan.Parse(startRaw!);
                    end = TimeSpan.Parse(endRaw!);
                }
                catch (FormatException ex)
                {
                    throw new ArgumentException($"Day at index {i}: invalid Start or End time.", ex);
                }

                if (start >= end)
                    throw new ArgumentException($"Day at index {i}: Start must be before End.");

                var calendarDate = dto.StartDate.AddDays(i).ToDateTime(TimeOnly.MinValue);
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

        public async Task<List<WorkingHoursDTO>> GetInRange(int artistId, WorkingHoursRangeRequestDTO dto)
        {
            if (artistId <= 0)
                throw new ArgumentException("Artist id must be positive.", nameof(artistId));
            if (dto.StartDate == default)
                throw new ArgumentException("StartDate is required.", nameof(dto.StartDate));
            if (dto.EndDate == default)
                throw new ArgumentException("EndDate is required.", nameof(dto.EndDate));
            if (dto.StartDate.Date > dto.EndDate.Date)
                throw new ArgumentException("StartDate must be earlier than or equal to EndDate.");

            var rows = await _workingHoursRepository.GetByArtistAndDateRange(
                artistId,
                dto.StartDate.Date,
                dto.EndDate.Date);

            var existingByDate = rows
                .GroupBy(w => w.Date.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.First());

            var result = new List<WorkingHoursDTO>();
            for (var day = dto.StartDate.Date; day <= dto.EndDate.Date; day = day.AddDays(1))
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
}
