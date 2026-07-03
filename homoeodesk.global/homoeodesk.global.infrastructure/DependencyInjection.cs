using HomoeoDesk.Global.Application.Common.Interfaces;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Infrastructure.Data;
using HomoeoDesk.Global.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomoeoDesk.Global.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var globalConnectionString = configuration.GetConnectionString("GlobalConnection")
            ?? throw new InvalidOperationException("GlobalConnection connection string is required in appsettings.json");

        services.AddDbContext<GlobalDbContext>(options =>
            options.UseSqlServer(globalConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            }));

        services.AddScoped<IGlobalDbContext>(provider => provider.GetRequiredService<GlobalDbContext>());
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpContextAccessor();

        return services;
    }
}
