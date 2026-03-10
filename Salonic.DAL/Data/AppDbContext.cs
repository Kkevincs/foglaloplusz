using Microsoft.EntityFrameworkCore;
using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Data
{
    public class AppDbContext : DbContext
    {

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Service> Services { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


    }
}
