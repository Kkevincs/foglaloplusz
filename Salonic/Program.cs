
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Foglaloplusz.BLL.Profiles;
using Foglaloplusz.BLL.Services;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Data;
using Foglaloplusz.DAL.Repositories;
using Foglaloplusz.DAL.Repositories.Interfaces;
using Foglaloplusz.FileStorage;
using System.Security.Claims;

namespace Foglaloplusz
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

            builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
            builder.Services.AddScoped<IOwnerService, OwnerService>();

            builder.Services.AddScoped<ISalonRepository, SalonRepository>();
            builder.Services.AddScoped<ISalonService, SalonService>();
            builder.Services.AddScoped<SalonImageStorage>();
            builder.Services.AddScoped<ISalonImageStorage>(sp => sp.GetRequiredService<SalonImageStorage>());
            builder.Services.AddScoped<ISalonLogoCleanup>(sp => sp.GetRequiredService<SalonImageStorage>());

            builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

            builder.Services.AddScoped<IWorkingHoursRepository, WorkingHoursRepository>();
            builder.Services.AddScoped<IWorkingHoursService, WorkingHoursService>();
            builder.Services.AddScoped<IArtistScheduleService, ArtistScheduleService>();
            builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
            //IServiceRepo
            //mapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile(new AppointmentProfile());
                cfg.AddProfile(new ArtistProfile());
                cfg.AddProfile(new GuestProfile());
                cfg.AddProfile(new OwnerProfile());
                cfg.AddProfile(new SalonProfile());
                cfg.AddProfile(new ServiceProfile());
                cfg.AddProfile(new UserProfile());
            });
            var connectionString = builder.Configuration.GetConnectionString("AppConnectionString");
            builder.Services.AddDbContext<AppDbContext>(options =>
               options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
               mysqloptions => mysqloptions.MigrationsAssembly("Foglaloplusz.DAL")
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
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
            });

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false; 
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Authentication:Issuer"],
                        ValidAudience = builder.Configuration["Authentication:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"])),
                        RoleClaimType = "role"
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = null;
            });



            var app = builder.Build();

            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
            Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "salons"));
            
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            

            app.Run();
        }
    }
}
