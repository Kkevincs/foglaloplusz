using Salonic.DAL.Models;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IWorkingHoursRepository
    {
        Task<WorkingHours?> GetByArtistAndDate(int artistId, DateTime date);
        Task<List<WorkingHours>> GetByArtistAndDateRange(int artistId, DateTime startDate, DateTime endDate);
        Task Add(WorkingHours workingHours);
        Task Update(WorkingHours workingHours);
        Task ReplaceForArtistDateRangeAsync(int artistId, DateOnly rangeStart, DateOnly rangeEnd, IReadOnlyList<WorkingHours> newRows);
    }
}
