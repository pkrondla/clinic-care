using ClinicCare.Application.Features.Sms.Commands.CreateOrUpdateSmsSettings;
using ClinicCare.Application.Features.Sms.Queries.GetSmsSettings;
using MediatR;

namespace ClinicCare.API.Modules.Settings;

public static class SmsEndpoints
{
    public static IEndpointRouteBuilder MapSmsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings/sms")
            .WithTags("SMS Settings")
            .WithOpenApi()
            .RequireAuthorization();

        // Get SMS settings
        group.MapGet("/", GetSmsSettings)
            .WithName("GetSmsSettings")
            .WithSummary("Get SMS settings for current organization")
            .Produces<SmsSettingsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Create or Update SMS settings
        group.MapPost("/", CreateOrUpdateSmsSettings)
            .WithName("CreateOrUpdateSmsSettings")
            .WithSummary("Create or update SMS settings (Admin only)")
            .Produces<SmsSettingsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetSmsSettings(IMediator mediator)
    {
        var query = new GetSmsSettingsQuery();
        var result = await mediator.Send(query);

        if (result == null)
        {
            return Results.NotFound(new { success = false, message = "SMS settings not found" });
        }

        return Results.Ok(new { success = true, data = result });
    }

    private static async Task<IResult> CreateOrUpdateSmsSettings(
        IMediator mediator,
        CreateOrUpdateSmsSettingsCommand command)
    {
        try
        {
            var result = await mediator.Send(command);

            return Results.Ok(new
            {
                success = true,
                data = result,
                message = "SMS settings saved successfully"
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

