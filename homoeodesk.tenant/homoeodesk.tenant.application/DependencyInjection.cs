using System.Reflection;
using HomoeoDesk.Tenant.Application.Common.Behaviours;
using HomoeoDesk.Tenant.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HomoeoDesk.Tenant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TenantBehaviour<,>));
        });

        // Shared read/write services (avoid MediatR command-calling-command chains)
        services.AddScoped<IInvoiceReadService, InvoiceReadService>();
        services.AddScoped<IInvoicePaymentService, InvoicePaymentService>();
        services.AddScoped<IPurchaseOrderReadService, PurchaseOrderReadService>();
        services.AddScoped<INotificationPreferencesReadService, NotificationPreferencesReadService>();

        return services;
    }
}
