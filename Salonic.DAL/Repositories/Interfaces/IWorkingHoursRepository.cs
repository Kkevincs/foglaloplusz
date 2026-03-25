using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IWorkingHoursRepository
    {
        Task<WorkingHours?> GetByArtistAndDate(int artistId, DateTime date);
        Task Add(WorkingHours workingHours);
        Task Update(WorkingHours workingHours);
    }
}
