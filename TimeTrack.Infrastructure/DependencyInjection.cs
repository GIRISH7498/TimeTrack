using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Infrastructure.Identity;
using TimeTrack.Infrastructure.Persistence;
using TimeTrack.Infrastructure.Services;

namespace TimeTrack.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("TimeTrackDBConnection")));

            services
                .AddIdentity<AppUser, AppRole>(options =>
                {
                    // configure password, lockout options etc. here if you want
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IIdentityService, IdentityService>();

            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddScoped<IBellNotificationService, BellNotificationService>();

            return services;
        }
    }
}
