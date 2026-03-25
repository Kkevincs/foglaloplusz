using Salonic.DAL.Data;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Salonic.DAL.Repositories
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
    }
}
