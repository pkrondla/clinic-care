using ClinicCare.Application;
using ClinicCare.Infrastructure;
using ClinicCare.API.Middleware;
using ClinicCare.API.Hubs;
using ClinicCare.API.Endpoints;
using ClinicCare.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Serilog;
using Hangfire;
using Hangfire.SqlServer;
using ClinicCare.API.Filters;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",     // Vite dev server default
                "http://localhost:3000",     // Create React App default
                "http://localhost:4173",     // Vite preview
                "http://localhost:51537",    // Current .NET API port
                "https://localhost:5173",    // HTTPS variants
                "https://localhost:3000",
                "https://localhost:4173",
                "https://localhost:51537"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()              // Required for SignalR
            .WithExposedHeaders("Content-Disposition", "X-Pagination") // For file downloads and pagination
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // Cache preflight for 10 minutes
    });
    
    // Development policy for broader access
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add SignalR with CORS support
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Add custom headers for API
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
        
        // Configure JWT for SignalR
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/queueHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Add Application Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Hangfire for background jobs
var hangfireConnectionString = builder.Configuration.GetConnectionString("GlobalConnection")
    ?? throw new InvalidOperationException("GlobalConnection connection string is required for Hangfire");

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
    options.Queues = new[] { "default", "notifications", "reports" };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Add security headers
app.Use(async (context, next) =>
{
    try
    {
        Console.WriteLine($"SecurityHeaders: Processing {context.Request.Method} {context.Request.Path}");
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        
        Console.WriteLine($"SecurityHeaders: About to call next() for {context.Request.Method} {context.Request.Path}");
        await next();
        Console.WriteLine($"SecurityHeaders: Completed {context.Request.Method} {context.Request.Path}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"SecurityHeaders: EXCEPTION for {context.Request.Method} {context.Request.Path}: {ex.GetType().Name} - {ex.Message}");
        Console.WriteLine($"SecurityHeaders: Stack trace: {ex.StackTrace}");
        throw; // Re-throw to let other middleware handle it
    }
});

// Use appropriate CORS policy based on environment
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Program.cs: Using CORS AllowAll policy");
    
    // First, register the built-in CORS middleware
    app.UseCors("AllowAll");
    
    // Then, add our wrapper AFTER the built-in CORS to catch any exceptions
    app.Use(async (context, next) =>
    {
        try
        {
            Console.WriteLine($"CORS: Processing {context.Request.Method} {context.Request.Path}");
            Console.WriteLine($"CORS: About to call next() for {context.Request.Method} {context.Request.Path}");
            
            await next();
            Console.WriteLine($"CORS: Completed {context.Request.Method} {context.Request.Path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CORS: EXCEPTION for {context.Request.Method} {context.Request.Path}: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"CORS: Stack trace: {ex.StackTrace}");
            throw; // Re-throw to let other middleware handle it
        }
    });
}
else
{
    Console.WriteLine("Program.cs: Using CORS AllowReactApp policy");
    app.UseCors("AllowReactApp");
}

// Production-ready middleware pipeline
Console.WriteLine("Program.cs: Setting up production middleware pipeline...");

// Add TenantMiddleware with factory pattern
app.Use(async (context, next) =>
{
    try
    {
        Console.WriteLine($"TenantMiddleware: ENTERED for path: {context.Request.Path}");
        
        // Skip tenant resolution for certain paths
        if (ShouldSkipTenantResolution(context.Request.Path))
        {
            Console.WriteLine($"TenantMiddleware: SKIPPING tenant resolution for path: {context.Request.Path}");
            await next();
            Console.WriteLine($"TenantMiddleware: Completed for path: {context.Request.Path}");
            return;
        }


        // Resolve tenant service from request scope
        Console.WriteLine($"TenantMiddleware: About to resolve ITenantService...");
        var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
        Console.WriteLine($"TenantMiddleware: ITenantService resolved successfully");
        
        Console.WriteLine($"TenantMiddleware: About to call GetOrganizationIdAsync()...");
        var organizationId = await tenantService.GetOrganizationIdAsync();
        Console.WriteLine($"TenantMiddleware: GetOrganizationIdAsync() completed, Organization ID: {organizationId}");
        
        Console.WriteLine($"TenantMiddleware: About to call next()...");
        await next();
        Console.WriteLine($"TenantMiddleware: Completed for path: {context.Request.Path}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"TenantMiddleware: EXCEPTION for path: {context.Request.Path}: {ex.GetType().Name} - {ex.Message}");
        Console.WriteLine($"TenantMiddleware: Stack trace: {ex.StackTrace}");
        throw;
    }
});

// Helper method to determine if tenant resolution should be skipped
static bool ShouldSkipTenantResolution(PathString path)
{
    var skipPaths = new[]
    {
        "/health",
        "/swagger",
        "/api/auth/login",   // Skip login endpoint - doesn't need tenant context
        "/api/auth/me",      // Skip only specific auth endpoints that don't need tenant context
        "/api/auth/refresh", // Skip only specific auth endpoints that don't need tenant context
        "/api/global",
        "/hangfire" // Skip tenant resolution for Hangfire dashboard
    };

    // Normalize the path
    var pathValue = path.Value?.ToLowerInvariant() ?? "";
    
    Console.WriteLine($"TenantMiddleware: Checking path: '{pathValue}'");
    
    // Check if path starts with any skip path
    var shouldSkip = skipPaths.Any(skipPath => 
        pathValue.StartsWith(skipPath.ToLowerInvariant()));
    
    Console.WriteLine($"TenantMiddleware: Should skip: {shouldSkip}");
    
    return shouldSkip;
}

// Add ExceptionMiddleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Map all minimal API endpoints
app.MapAllEndpoints();

// SignalR Hubs
app.MapHub<QueueHub>("/queueHub");

// Hangfire Dashboard (only in development or for authorized users)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() },
        DashboardTitle = "ClinicCare Background Jobs"
    });
}
else
{
    // In production, add proper authorization
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() },
        DashboardTitle = "ClinicCare Background Jobs"
    });
}

// Seed database (disabled - databases are already seeded via SQL scripts)
// If you need to re-seed, uncomment this section and ensure proper database access
/*
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await ClinicCare.Infrastructure.Data.Seeders.DatabaseSeeder.SeedAsync(context, passwordHasher);
        Log.Information("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database seeding skipped (databases already seeded via SQL scripts)");
    }
}
*/
Log.Information("Database seeding skipped - databases are pre-seeded via SQL scripts");

// Schedule recurring background jobs
using (var scope = app.Services.CreateScope())
{
    try
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var notificationJobs = scope.ServiceProvider.GetRequiredService<ClinicCare.Infrastructure.Jobs.NotificationJobs>();

        // Schedule appointment reminders - run every hour
        recurringJobManager.AddOrUpdate(
            "appointment-reminders",
            () => notificationJobs.SendAppointmentRemindersAsync(CancellationToken.None),
            Cron.Hourly);

        // Schedule token status updates - run every 5 minutes
        recurringJobManager.AddOrUpdate(
            "token-status-updates",
            () => notificationJobs.SendTokenStatusUpdatesAsync(CancellationToken.None),
            "*/5 * * * *"); // Every 5 minutes

        Log.Information("Hangfire background jobs scheduled successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to schedule Hangfire background jobs - continuing without scheduled jobs");
    }
}

try
{
    Log.Information("Starting ClinicCare API...");
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

