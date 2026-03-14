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
    public class GuestRepository : IGuestRepository
    {

        private readonly AppDbContext _context;

        public GuestRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guest> Create(Guest guest)
        {
            _context.Guests.Add(guest);
            _context.SaveChanges();
            return guest;
        }

        public async Task DeleteById(int id)
        {
            Guest Guest =  _context.Guests.Where(x => x.Id == id).FirstOrDefault();
            _context.Remove(Guest);
            _context.SaveChanges();
        }

        public async Task<List<Guest>> GetAll()
        {
            return _context.Guests.Select(x => x).ToList();
        }

        public async Task<Guest> GetById(int id)
        {
            return _context.Guests.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
        }

        public async Task Update(Guest guest)
        {
            _context.Guests.Update(guest);
            await _context.SaveChangesAsync();
        }
    }
}
