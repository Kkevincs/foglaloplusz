using Microsoft.EntityFrameworkCore;
using Salonic.Models;

namespace Salonic.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<Worker> Workers => Set<Worker>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ----- GUEST -----
            modelBuilder.Entity<Guest>(e =>
            {
                e.HasKey(g => g.Id);
                e.Property(g => g.Name).IsRequired().HasMaxLength(200);
                e.Property(g => g.Email).IsRequired().HasMaxLength(256);
                e.Property(g => g.Pwd).IsRequired().HasMaxLength(500);
            });

            // ----- WORKER -----
            modelBuilder.Entity<Worker>(e =>
            {
                e.HasKey(w => w.Id);
                e.Property(w => w.Name).IsRequired().HasMaxLength(200);
                e.Property(w => w.Email).IsRequired().HasMaxLength(256);
                e.Property(w => w.Pwd).IsRequired().HasMaxLength(500);
            });

            // ----- SERVICE -----
            modelBuilder.Entity<Service>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            });

            // ----- WORKER ↔ SERVICE (many-to-many; EF creates junction table automatically) -----
            modelBuilder.Entity<Worker>()
                .HasMany(w => w.Services)
                .WithMany(s => s.Workers);

            // ----- APPOINTMENTS (links Guest, Worker, Service) -----
            modelBuilder.Entity<Appointment>(e =>
            {
                e.HasKey(a => a.Id);
                e.Property(a => a.Notes).HasMaxLength(2000);

                // GUEST → APPOINTMENTS (1 : N): one guest can have many appointments
                e.HasOne(a => a.Guest)
                    .WithMany(g => g.Appointments)
                    .HasForeignKey(a => a.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);

                // WORKER → APPOINTMENTS (1 : N): one worker can have many appointments
                e.HasOne(a => a.Worker)
                    .WithMany(w => w.Appointments)
                    .HasForeignKey(a => a.WorkerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // SERVICE → APPOINTMENTS (1 : N): one service can appear in many appointments
                e.HasOne(a => a.Service)
                    .WithMany(s => s.Appointments)
                    .HasForeignKey(a => a.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
