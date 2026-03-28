using AutoMapper;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic.BLL.Services.Interfaces
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
    }
}
