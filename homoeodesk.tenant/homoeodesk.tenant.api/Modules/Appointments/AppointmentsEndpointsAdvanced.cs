using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointmentStats;
using HomoeoDesk.Tenant.Application.Common.Models;

namespace HomoeoDesk.Tenant.Api.Modules.Appointments;

public static class AppointmentsEndpointsAdvanced
{
    public static IEndpointRouteBuilder MapAppointmentsAdvancedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments - Advanced")
            .WithOpenApi()
            .RequireAuthorization();

        // Get appointments with advanced filtering and caching
        group.MapGet("/search", SearchAppointments)
            .WithName("SearchAppointments")
            .WithSummary("Search appointments with advanced filtering")
            .WithDescription("Search appointments with multiple filters, sorting, and pagination")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .CacheOutput(policy => policy
                .Expire(TimeSpan.FromMinutes(5))
                .SetVaryByQuery("*"));

        // Get appointment statistics with caching
        group.MapGet("/analytics", GetAppointmentAnalytics)
            .WithName("GetAppointmentAnalytics")
            .WithSummary("Get detailed appointment analytics")
            .WithDescription("Get comprehensive appointment analytics and insights")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .CacheOutput(policy => policy
                .Expire(TimeSpan.FromMinutes(10))
                .SetVaryByQuery("*"));

        // Get doctor queue with real-time updates
        group.MapGet("/queue/{doctorId:int}/{BranchId:int}", GetDoctorQueue)
            .WithName("GetDoctorQueue")
            .WithSummary("Get doctor's appointment queue")
            .WithDescription("Get real-time appointment queue for a specific doctor and clinic")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Export appointments
        group.MapGet("/export", ExportAppointments)
            .WithName("ExportAppointments")
            .WithSummary("Export appointments to various formats")
            .WithDescription("Export appointments to CSV, Excel, or PDF formats")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> SearchAppointments(
        int? BranchId,
        int? doctorId,
        int? patientId,
        DateOnly? startDate,
        DateOnly? endDate,
        int? status,
        int? type,
        string? sortBy,
        string? sortOrder,
        int page = 1,
        int pageSize = 20,
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAppointmentsQuery(BranchId, doctorId, startDate, status);
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to search appointments", errors = result.Errors });
        }

        // Apply additional filtering
        var filteredData = result.Data.AsQueryable();

        if (patientId.HasValue)
            filteredData = filteredData.Where(x => x.Patient.Id == patientId.Value);

        if (type.HasValue)
            filteredData = filteredData.Where(x => x.Type == type.Value);

        if (endDate.HasValue && startDate.HasValue)
            filteredData = filteredData.Where(x => x.AppointmentDate >= startDate.Value && x.AppointmentDate <= endDate.Value);

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            var isDescending = sortOrder?.ToLower() == "desc";
            filteredData = sortBy.ToLower() switch
            {
                "date" => isDescending ? filteredData.OrderByDescending(x => x.AppointmentDate) : filteredData.OrderBy(x => x.AppointmentDate),
                "token" => isDescending ? filteredData.OrderByDescending(x => x.TokenNumber) : filteredData.OrderBy(x => x.TokenNumber),
                "status" => isDescending ? filteredData.OrderByDescending(x => x.Status) : filteredData.OrderBy(x => x.Status),
                _ => filteredData.OrderBy(x => x.AppointmentDate)
            };
        }

        // Apply pagination
        var totalCount = filteredData.Count();
        var pagedData = filteredData
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Results.Ok(new
        {
            data = pagedData,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            }
        });
    }

    private static async Task<IResult> GetAppointmentAnalytics(
        int? BranchId,
        int? doctorId,
        DateOnly? startDate,
        DateOnly? endDate,
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAppointmentStatsQuery(BranchId, doctorId);
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve analytics", errors = result.Errors });
        }

        // Add additional analytics
        var analytics = new
        {
            basic = result.Data,
            trends = new
            {
                // TODO: Add trend analysis
                message = "Trend analysis coming soon"
            },
            insights = new
            {
                // TODO: Add insights
                message = "Insights coming soon"
            }
        };

        return Results.Ok(new { data = analytics });
    }

    private static async Task<IResult> GetDoctorQueue(
        int doctorId,
        int BranchId,
        DateOnly? date,
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        
        var query = new GetAppointmentsQuery(null, doctorId, targetDate, null);
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to retrieve doctor queue", errors = result.Errors });
        }

        // Filter for queue (scheduled and in-progress only)
        var queue = result.Data
            .Where(x => x.Status == 1 || x.Status == 2) // Scheduled or InProgress
            .OrderBy(x => x.TokenNumber)
            .Select((appointment, index) => new
            {
                appointment.Id,
                appointment.TokenNumber,
                appointment.Status,
                appointment.Type,
                appointment.Patient,
                QueuePosition = index + 1,
                EstimatedWaitTime = CalculateEstimatedWaitTime(index)
            })
            .ToList();

        return Results.Ok(new { data = queue });
    }

    private static async Task<IResult> ExportAppointments(
        int? BranchId,
        int? doctorId,
        DateOnly? startDate,
        DateOnly? endDate,
        string format = "csv",
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAppointmentsQuery(BranchId, doctorId, startDate, null);
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to export appointments", errors = result.Errors });
        }

        // TODO: Implement actual export logic
        return Results.Ok(new { message = $"Export to {format} coming soon", data = result.Data });
    }

    private static string CalculateEstimatedWaitTime(int queuePosition)
    {
        // Simple calculation: 15 minutes per appointment
        var estimatedMinutes = queuePosition * 15;
        return $"{estimatedMinutes} minutes";
    }
}
