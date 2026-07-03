using HomoeoDesk.Tenant.Application;
using HomoeoDesk.Tenant.Infrastructure;
using HomoeoDesk.Tenant.Api.Middleware;
using HomoeoDesk.Tenant.Api.Hubs;
using HomoeoDesk.Tenant.Api.Endpoints;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Api.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Serilog;
using Hangfire;
using Hangfire.SqlServer;
using HomoeoDesk.Tenant.Api.Filters;
using HomoeoDesk.Tenant.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);
builder.AddHomoeoDeskProductionServices();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHomoeoDeskCorsPolicies(builder.Configuration);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/queueHub"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<HomoeoDesk.Tenant.Application.Common.Services.IQueueNotificationService, HomoeoDesk.Tenant.Api.Services.QueueNotificationService>();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();

var hangfireConnectionString = builder.Configuration.GetConnectionString("TenantConnection")
    ?.Replace("{TenantId}", builder.Configuration["TenantStamp:FixedTenantSubdomain"] ?? "demo")
    ?? throw new InvalidOperationException("TenantConnection connection string is required for Hangfire");

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 5;
    options.Queues = ["tenant-default", "tenant-notifications", "tenant-maintenance"];
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

app.UseHomoeoDeskCors();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<TrialExpiredMiddleware>();
app.UseMiddleware<BranchMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapAllEndpoints();
app.MapHub<QueueHub>("/queueHub");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()],
    DashboardTitle = "HomoeoDesk Tenant Background Jobs"
});

using (var scope = app.Services.CreateScope())
{
    try
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var notificationJobs = scope.ServiceProvider.GetRequiredService<NotificationJobs>();

        recurringJobManager.AddOrUpdate(
            "appointment-reminders",
            () => notificationJobs.SendAppointmentRemindersAsync(CancellationToken.None),
            Cron.Hourly,
            new RecurringJobOptions { QueueName = "tenant-notifications" });

        recurringJobManager.AddOrUpdate(
            "token-status-updates",
            () => notificationJobs.SendTokenStatusUpdatesAsync(CancellationToken.None),
            "*/5 * * * *",
            new RecurringJobOptions { QueueName = "tenant-notifications" });

        recurringJobManager.AddOrUpdate(
            "database-maintenance",
            () => notificationJobs.RunDatabaseMaintenanceAsync(CancellationToken.None),
            Cron.Daily,
            new RecurringJobOptions { QueueName = "tenant-maintenance" });

        Log.Information("Hangfire tenant background jobs scheduled successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to schedule Hangfire background jobs");
    }
}

try
{
    Log.Information("Starting HomoeoDesk Tenant API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
