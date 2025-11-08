using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Infrastructure.Data;
using ClinicCare.Infrastructure.Data.Repositories;
using ClinicCare.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register IApplicationDbContext as the concrete ApplicationDbContext
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        // Repositories
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Services
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        
        services.AddHttpContextAccessor();

        return services;
    }
}
