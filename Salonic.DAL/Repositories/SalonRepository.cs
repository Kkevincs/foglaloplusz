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
    public class SalonRepository : ISalonRepository
    {

        private readonly AppDbContext _context;

        public SalonRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Salon> Create(Salon salon)
        {
            _context.Salons.Add(salon);
            _context.SaveChanges();
            return salon;
        }

        public async Task DeleteById(int id)
        {
            Salon salon =  _context.Salons.Where(x => x.Id == id).FirstOrDefault();
            _context.Remove(salon);
            _context.SaveChanges();
        }

        public async Task<List<Salon>> GetAll()
        {
            return _context.Salons.Select(x => x).ToList();
        }

        public async Task<Salon> GetById(int id)
        {
            return _context.Salons.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
        }

        public async Task Update(Salon salon)
        {
            _context.Salons.Update(salon);
            await _context.SaveChangesAsync();
        }
    }
}
