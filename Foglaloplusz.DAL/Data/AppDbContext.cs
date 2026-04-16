using Microsoft.EntityFrameworkCore;
using Foglaloplusz.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasMany(a => a.Services)
                .WithMany(s => s.Appointments)
                .UsingEntity(j => j.ToTable("AppointmentServices"));

            modelBuilder.Entity<Service>()
                .HasOne(s => s.Salon)
                .WithMany()
                .HasForeignKey(s => s.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Service>()
                .HasOne(s => s.CreatedByArtist)
                .WithMany()
                .HasForeignKey(s => s.CreatedByArtistId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
