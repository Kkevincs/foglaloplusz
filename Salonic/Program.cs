
using Microsoft.EntityFrameworkCore;
using Salonic.Data;

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

            builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

            builder.Services.AddScoped<IRegisterService, RegisterService>();
            //IServiceRepo
            //mapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile(new AppointmentProfile());
                cfg.AddProfile(new ArtistProfile());
                cfg.AddProfile(new GuestProfile());
                cfg.AddProfile(new ServiceProfile());
                cfg.AddProfile(new UserProfile());
            });
            var connectionString = builder.Configuration.GetConnectionString("AppConnectionString");
            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(builder => builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
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

            app.UseCors("AllowAll");

            

            app.UseAuthorization();
                    
            app.MapControllers();

            app.Run();
        }
    }
}
