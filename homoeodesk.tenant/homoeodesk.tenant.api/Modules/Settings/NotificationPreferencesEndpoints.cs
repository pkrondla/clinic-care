using GetNotificationPreferenceDto = HomoeoDesk.Tenant.Application.Features.Notifications.Queries.GetNotificationPreferences.NotificationPreferenceDto;
using HomoeoDesk.Tenant.Application.Features.Notifications.Commands.UpdateNotificationPreferences;
using HomoeoDesk.Tenant.Application.Features.Notifications.Queries.GetNotificationPreferences;
using MediatR;

namespace HomoeoDesk.Tenant.Api.Modules.Settings;

public static class NotificationPreferencesEndpoints
{
    public static IEndpointRouteBuilder MapNotificationPreferencesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings/notifications")
            .WithTags("Notification Preferences")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all notification preferences
        group.MapGet("/", GetNotificationPreferences)
            .WithName("GetNotificationPreferences")
            .WithSummary("Get notification preferences for current organization")
            .Produces<List<GetNotificationPreferenceDto>>(StatusCodes.Status200OK);

        // Update notification preferences
        group.MapPut("/", UpdateNotificationPreferences)
            .WithName("UpdateNotificationPreferences")
            .WithSummary("Update notification preferences (Admin only)")
            .Produces<List<GetNotificationPreferenceDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetNotificationPreferences(IMediator mediator)
    {
        var query = new GetNotificationPreferencesQuery();
        var result = await mediator.Send(query);

        return Results.Ok(new { success = true, data = result });
    }

    private static async Task<IResult> UpdateNotificationPreferences(
        IMediator mediator,
        UpdateNotificationPreferencesCommand command)
    {
        try
        {
            var result = await mediator.Send(command);

            return Results.Ok(new
            {
                success = true,
                data = result,
                message = "Notification preferences updated successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { success = false, message = ex.Message });
        }
    }
}

