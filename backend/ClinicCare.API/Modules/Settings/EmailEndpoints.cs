using ClinicCare.Application.Features.Email.Commands.CreateOrUpdateEmailSettings;
using ClinicCare.Application.Features.Email.Queries.GetEmailSettings;
using MediatR;

namespace ClinicCare.API.Modules.Settings;

public static class EmailEndpoints
{
    public static IEndpointRouteBuilder MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings/email")
            .WithTags("Email Settings")
            .WithOpenApi()
            .RequireAuthorization();

        // Get Email settings
        group.MapGet("/", GetEmailSettings)
            .WithName("GetEmailSettings")
            .WithSummary("Get Email settings for current organization")
            .Produces<EmailSettingsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Create or Update Email settings
        group.MapPost("/", CreateOrUpdateEmailSettings)
            .WithName("CreateOrUpdateEmailSettings")
            .WithSummary("Create or update Email settings (Admin only)")
            .Produces<EmailSettingsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetEmailSettings(IMediator mediator)
    {
        var query = new GetEmailSettingsQuery();
        var result = await mediator.Send(query);

        if (result == null)
        {
            return Results.NotFound(new { success = false, message = "Email settings not found" });
        }

        return Results.Ok(new { success = true, data = result });
    }

    private static async Task<IResult> CreateOrUpdateEmailSettings(
        IMediator mediator,
        CreateOrUpdateEmailSettingsCommand command)
    {
        try
        {
            var result = await mediator.Send(command);

            return Results.Ok(new
            {
                success = true,
                data = result,
                message = "Email settings saved successfully"
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

