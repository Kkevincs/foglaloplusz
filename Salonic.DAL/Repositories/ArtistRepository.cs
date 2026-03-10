using Salonic.DAL.Data;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories
{
    public class ArtistRepository : IArtistRepository
    {

        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Artist> Create(Artist artist)
        {
            _context.Artists.Add(artist);
            _context.SaveChanges();
            return artist;
        }

        public async Task DeleteById(int id)
        {
           var artist = _context.Artists.Where(x => x.Id == id);
            _context.Remove(artist);
            _context.SaveChanges();
        }

        public async Task<List<Artist>> GetAll()
        {
            return _context.Artists.Select(x => x).ToList();
        }

        public async Task<Artist> GetById(int id)
        {
            return _context.Artists.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
        }

        public async Task Update(Artist artist)
        {
           _context.Artists.Update(artist);
            await _context.SaveChangesAsync();
        }
    }
}
