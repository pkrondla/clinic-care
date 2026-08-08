using HomoeoDesk.Global.Application.Features.PublicRegistration.Commands.RegisterTrialOrganization;
using HomoeoDesk.Global.Application.Features.TrialRequests.Commands.CreateTrialRequest;
using MediatR;

namespace HomoeoDesk.Global.Api.Modules.Public;

public static class PublicEndpoints
{
    public static IEndpointRouteBuilder MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public")
            .WithTags("Public")
            .WithOpenApi()
            .AllowAnonymous();

        group.MapPost("/trial-requests", CreateTrialRequest)
            .WithName("CreatePublicTrialRequest")
            .WithSummary("Capture a trial / sales lead from the marketing website")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPost("/register", RegisterTrial)
            .WithName("RegisterTrialOrganization")
            .WithSummary("Self-serve trial organization registration")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> CreateTrialRequest(IMediator mediator, CreateTrialRequestCommand command)
    {
        var result = await mediator.Send(command);
        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Trial request received" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> RegisterTrial(IMediator mediator, RegisterTrialOrganizationCommand command)
    {
        var result = await mediator.Send(command);
        return result.Succeeded
            ? Results.Ok(new
            {
                success = true,
                data = result.Data,
                message = "Trial organization registered. Our team will complete provisioning and email next steps."
            })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}
