using Foglaloplusz.DAL.Data;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Foglaloplusz.DAL.Repositories
{
    public class WorkingHoursRepository : IWorkingHoursRepository
    {
        private readonly AppDbContext _context;
        public WorkingHoursRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WorkingHours?> GetByArtistAndDate(int artistId, DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            return await _context.WorkingHours
                .FirstOrDefaultAsync(w =>
                    w.ArtistId == artistId &&
                    w.Date >= dayStart &&
                    w.Date < dayEnd);
        }

        public async Task<List<WorkingHours>> GetByArtistAndDateRange(int artistId, DateTime startDate, DateTime endDate)
        {
            var rangeStart = startDate.Date;
            var rangeEndExclusive = endDate.Date.AddDays(1);

            return await _context.WorkingHours
                .Where(w =>
                    w.ArtistId == artistId &&
                    w.Date >= rangeStart &&
                    w.Date < rangeEndExclusive)
                .OrderBy(w => w.Date)
                .ToListAsync();
        }

        public async Task Add(WorkingHours workingHours)
        {
            _context.WorkingHours.Add(workingHours);
            await _context.SaveChangesAsync();
        }

        public async Task Update(WorkingHours workingHours)
        {
            _context.WorkingHours.Update(workingHours);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceForArtistDateRangeAsync(int artistId, DateOnly rangeStart, DateOnly rangeEnd, IReadOnlyList<WorkingHours> newRows)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var startDt = rangeStart.ToDateTime(TimeOnly.MinValue);
                var endExclusive = rangeEnd.ToDateTime(TimeOnly.MinValue).AddDays(1);

                await _context.WorkingHours
                    .Where(w => w.ArtistId == artistId && w.Date >= startDt && w.Date < endExclusive)
                    .ExecuteDeleteAsync();

                if (newRows.Count > 0)
                    _context.WorkingHours.AddRange(newRows);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
