using Microsoft.EntityFrameworkCore;

namespace Salonic.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Models.Appointment> Appointments { get; set; }
        public DbSet<Models.Service> Services { get; set; }
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.Worker> Workers { get; set; }
    }
}
