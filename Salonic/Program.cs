
using Microsoft.EntityFrameworkCore;
using Salonic.BLL.Profiles;
using Salonic.BLL.Services;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Data;
using Salonic.DAL.Repositories;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddScoped<IServiceService, ServiceService>();

            builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
            builder.Services.AddScoped<IArtistService, ArtistService>();

            builder.Services.AddScoped<IGuestRepository, GuestRepository>();
            builder.Services.AddScoped<IGuestService, GuestService>();

            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            //IServiceRepo
            //mapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile(new AppointmentProfile());
                cfg.AddProfile(new ArtistProfile());
                cfg.AddProfile(new GuestProfile());
                cfg.AddProfile(new ServiceProfile());
            });
            var connectionString = builder.Configuration.GetConnectionString("AppConnectionString");
            builder.Services.AddDbContext<AppDbContext>(options =>
               options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
               mysqloptions => mysqloptions.MigrationsAssembly("Salonic.DAL")
            ));
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseRouting();



            

            app.UseAuthorization();

            app.UseCors("AllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}
