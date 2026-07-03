using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Infrastructure.Configuration;
using HomoeoDesk.Tenant.Infrastructure.Data;
using HomoeoDesk.Tenant.Infrastructure.Jobs;
using HomoeoDesk.Tenant.Infrastructure.Services;
using HomoeoDesk.Tenant.Infrastructure.Services.PaymentGateways;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomoeoDesk.Tenant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TenantStampOptions>(
            configuration.GetSection(TenantStampOptions.SectionName));

        var tenantStampOptions = configuration
            .GetSection(TenantStampOptions.SectionName)
            .Get<TenantStampOptions>() ?? new TenantStampOptions();

        services.AddDbContext<TenantDbContext>((serviceProvider, options) =>
        {
            string connectionString;
            if (tenantStampOptions.EnableFixedTenant
                && !string.IsNullOrWhiteSpace(tenantStampOptions.FixedTenantConnectionString))
            {
                connectionString = tenantStampOptions.FixedTenantConnectionString;
            }
            else
            {
                var tenantService = serviceProvider.GetRequiredService<ITenantService>();
                var subdomain = tenantService.Subdomain ?? "demo";
                var baseConnectionString = configuration.GetConnectionString("TenantConnection")
                    ?? throw new InvalidOperationException("TenantConnection connection string is required in appsettings.json");
                connectionString = baseConnectionString.Replace("{TenantId}", subdomain);
            }

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            });
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<TenantDbContext>());

        services.AddScoped<IGlobalMedicineCatalogService, GlobalMedicineCatalogService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<Application.Common.Services.ITokenNumberService, TokenNumberService>();
        services.AddScoped<Application.Common.Services.IPdfService, PdfService>();

        services.AddDataProtection();
        services.AddScoped<Application.Common.Interfaces.IDataProtectionService, DataProtectionService>();

        services.AddHttpClient("MetaWhatsApp", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://graph.facebook.com/");
        });

        services.AddScoped<IWhatsAppProviderFactory, WhatsAppProviderFactory>();
        services.AddScoped<Application.Common.Services.IWhatsAppService, WhatsAppService>();
        services.AddScoped<Application.Common.Services.NotificationTemplateService>();
        services.AddScoped<Application.Common.Services.IEmailService, EmailService>();
        services.AddScoped<Application.Common.Services.ISmsService, SmsService>();
        services.AddScoped<Application.Common.Services.INotificationService, NotificationService>();

        services.AddScoped<PlaceholderPaymentGateway>();
        services.AddScoped<Application.Common.Services.IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<NotificationJobs>();

        services.AddHttpContextAccessor();

        return services;
    }
}
