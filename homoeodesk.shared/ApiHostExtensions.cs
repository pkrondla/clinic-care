using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomoeoDesk.Api.Shared;

public static class ApiHostExtensions
{
    public static WebApplicationBuilder AddHomoeoDeskProductionServices(this WebApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["KeyVault:Uri"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new Azure.Identity.DefaultAzureCredential());
        }

        var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });
        }

        return builder;
    }

    public static WebApplication UseHomoeoDeskCors(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowAll");
            return app;
        }

        var origins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins is { Length: > 0 })
        {
            app.UseCors("ConfiguredOrigins");
        }
        else
        {
            app.UseCors("AllowReactApp");
        }

        return app;
    }

    public static IServiceCollection AddHomoeoDeskCorsPolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:3000",
                        "http://localhost:4173",
                        "https://localhost:5173",
                        "https://localhost:3000",
                        "https://localhost:4173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition", "X-Pagination")
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });

            options.AddPolicy("ConfiguredOrigins", policy =>
            {
                policy.WithOrigins(configuredOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition", "X-Pagination")
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });

            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
