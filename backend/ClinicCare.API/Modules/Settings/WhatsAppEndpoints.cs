using ClinicCare.Application.Features.WhatsApp.Commands.CreateOrUpdateWhatsAppSettings;
using ClinicCare.Application.Features.WhatsApp.Commands.TestWhatsAppConnection;
using ClinicCare.Application.Features.WhatsApp.Queries.GetWhatsAppSettings;
using MediatR;

namespace ClinicCare.API.Modules.Settings;

public static class WhatsAppEndpoints
{
    public static IEndpointRouteBuilder MapWhatsAppEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings/whatsapp")
            .WithTags("WhatsApp Settings")
            .WithOpenApi()
            .RequireAuthorization();

        // Get WhatsApp settings
        group.MapGet("/", GetWhatsAppSettings)
            .WithName("GetWhatsAppSettings")
            .WithSummary("Get WhatsApp Business settings for current organization")
            .Produces<WhatsAppSettingsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Create or Update WhatsApp settings
        group.MapPost("/", CreateOrUpdateWhatsAppSettings)
            .WithName("CreateOrUpdateWhatsAppSettings")
            .WithSummary("Create or update WhatsApp Business settings (Admin only)")
            .Produces<WhatsAppSettingsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        // Test WhatsApp connection
        group.MapPost("/test", TestWhatsAppConnection)
            .WithName("TestWhatsAppConnection")
            .WithSummary("Test WhatsApp Business API connection")
            .Produces<TestWhatsAppConnectionResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetWhatsAppSettings(IMediator mediator)
    {
        var query = new GetWhatsAppSettingsQuery();
        var result = await mediator.Send(query);

        if (result == null)
        {
            return Results.NotFound(new { success = false, message = "WhatsApp settings not found" });
        }

        return Results.Ok(new { success = true, data = result });
    }

    private static async Task<IResult> CreateOrUpdateWhatsAppSettings(
        IMediator mediator, 
        CreateOrUpdateWhatsAppSettingsCommand command)
    {
        try
        {
            var result = await mediator.Send(command);

            return Results.Ok(new 
            { 
                success = true, 
                data = result, 
                message = "WhatsApp settings saved successfully" 
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

    private static async Task<IResult> TestWhatsAppConnection(IMediator mediator)
    {
        var command = new TestWhatsAppConnectionCommand();
        var result = await mediator.Send(command);

        if (result.Success)
        {
            return Results.Ok(new 
            { 
                success = true, 
                data = result,
                message = result.Message 
            });
        }
        else
        {
            return Results.BadRequest(new 
            { 
                success = false, 
                data = result,
                message = result.ErrorMessage ?? result.Message 
            });
        }
    }
}

