using Microsoft.EntityFrameworkCore;
using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Salon> Salons { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<Offering> Offerings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasMany(a => a.Services)
                .WithMany(s => s.Appointments)
                .UsingEntity(j => j.ToTable("AppointmentServices"));

            modelBuilder.Entity<Service>(e =>
            {
                e.HasOne(s => s.Salon)
                    .WithMany()
                    .HasForeignKey(s => s.SalonId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(s => s.Owner)
                    .WithMany()
                    .HasForeignKey(s => s.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(s => s.Artist)
                    .WithMany()
                    .HasForeignKey(s => s.ArtistId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Offering>(e =>
            {
                e.HasOne(o => o.Service)
                    .WithMany(s => s.Offerings)
                    .HasForeignKey(o => o.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(o => o.Artist)
                    .WithMany(a => a.Offerings)
                    .HasForeignKey(o => o.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(o => new { o.ArtistId, o.ServiceId }).IsUnique();
            });
        }
    }
}
