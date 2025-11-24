using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Infrastructure.Data;
using ClinicCare.Infrastructure.Data.Repositories;
using ClinicCare.Infrastructure.Data.Repositories.Global;
using ClinicCare.Infrastructure.Data.Repositories.Tenant;
using ClinicCare.Infrastructure.Services;
using ClinicCare.Infrastructure.Services.PaymentGateways;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Global Database Context
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

        // Tenant Database Context - connection string resolved dynamically per request
        // Use factory pattern to resolve connection string at request time (after tenant is resolved)
        services.AddScoped<TenantDbContext>(serviceProvider =>
        {
            var tenantService = serviceProvider.GetRequiredService<ITenantService>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            
            // Ensure subdomain is resolved (this will be set by TenantMiddleware)
            var subdomain = tenantService.Subdomain ?? "demo";
            
            // Get base connection string template
            var baseConnectionString = config.GetConnectionString("TenantConnection")
                ?? throw new InvalidOperationException("TenantConnection connection string is required in appsettings.json");
            
            // Replace placeholder with actual tenant subdomain
            var connectionString = baseConnectionString.Replace("{TenantId}", subdomain);
            
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            });
            
            return new TenantDbContext(optionsBuilder.Options);
        });

        // Legacy Database Context (for backward compatibility - uses tenant connection with default "demo" tenant)
        // Note: This should be migrated to use TenantDbContext in the future
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var tenantConnectionString = configuration.GetConnectionString("TenantConnection")
                ?? throw new InvalidOperationException("TenantConnection connection string is required in appsettings.json");
            // Use default "demo" tenant for legacy context
            var connectionString = tenantConnectionString.Replace("{TenantId}", "demo");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            });
        });

        // Register DB Context Interfaces
        services.AddScoped<IGlobalDbContext>(provider => provider.GetRequiredService<GlobalDbContext>());
        services.AddScoped<ITenantDbContext>(provider => provider.GetRequiredService<TenantDbContext>());
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        // Global Repositories
        services.AddScoped<IGlobalMedicineRepository, GlobalMedicineRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();

        // Tenant Repositories
        services.AddScoped<IClinicRepository, ClinicRepository>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IClinicMedicineRepository, ClinicMedicineRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Services
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ClinicCare.Application.Common.Services.ITokenNumberService, TokenNumberService>();
        services.AddScoped<ClinicCare.Application.Common.Services.IQueueNotificationService, QueueNotificationService>();
        services.AddScoped<ClinicCare.Application.Common.Services.IPdfService, PdfService>();
        services.AddScoped<ClinicCare.Application.Common.Services.IWhatsAppService, WhatsAppService>();
        services.AddScoped<ClinicCare.Application.Common.Services.IEmailService, EmailService>();
        services.AddScoped<ClinicCare.Application.Common.Services.ISmsService, SmsService>();
        services.AddScoped<ClinicCare.Application.Common.Services.INotificationService, NotificationService>();
        
        // Payment Gateway Services
        services.AddScoped<PlaceholderPaymentGateway>();
        services.AddScoped<ClinicCare.Application.Common.Services.IPaymentGatewayFactory, PaymentGatewayFactory>();
        
        // Background Jobs
        services.AddScoped<ClinicCare.Infrastructure.Jobs.NotificationJobs>();
        
        services.AddHttpContextAccessor();

        return services;
    }
}
