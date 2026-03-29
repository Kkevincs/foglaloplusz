using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IArtistRepository
    {
        public Task<List<Artist>> GetAll();
        Task<List<Artist>> GetBySalonId(int salonId);
        public Task<Artist> GetById(int id);
        public Task<Artist> Create(Artist artist);
        public Task Update(Artist artist);
        public Task DeleteById(int id);
    }
}
