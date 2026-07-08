using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Identity;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(options =>
            {
                var section = configuration.GetSection("JwtSettings");
                options.Secret = section["Secret"] ?? string.Empty;
                options.ExpiryMinutes = int.TryParse(section["ExpiryMinutes"], out var exp) ? exp : 15;
                options.RefreshTokenExpiryDays = int.TryParse(section["RefreshTokenExpiryDays"], out var rExp) ? rExp : 7;
                options.Issuer = section["Issuer"] ?? string.Empty;
                options.Audience = section["Audience"] ?? string.Empty;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IIdentityService, IdentityService>();

            return services;
        }
    }
}
