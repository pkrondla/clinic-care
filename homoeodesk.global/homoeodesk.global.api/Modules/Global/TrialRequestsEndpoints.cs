using HomoeoDesk.Global.Application.Features.TrialRequests.Commands.UpdateTrialRequestStatus;
using HomoeoDesk.Global.Application.Features.TrialRequests.Queries.GetTrialRequests;
using MediatR;

namespace HomoeoDesk.Global.Api.Modules.Global;

public static class TrialRequestsEndpoints
{
    public static IEndpointRouteBuilder MapTrialRequestsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/global/trial-requests")
            .WithTags("Trial Requests")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator, string? status) =>
        {
            var result = await mediator.Send(new GetTrialRequestsQuery { Status = status });
            return result.Succeeded
                ? Results.Ok(new { success = true, data = result.Data })
                : Results.BadRequest(new { success = false, errors = result.Errors });
        })
        .WithName("GetTrialRequests")
        .WithSummary("List trial requests for SuperAdmin ops");

        group.MapPatch("/{id:int}/status", async (IMediator mediator, int id, UpdateTrialRequestStatusCommand command) =>
        {
            command.Id = id;
            var result = await mediator.Send(command);
            return result.Succeeded
                ? Results.Ok(new { success = true, data = result.Data })
                : Results.BadRequest(new { success = false, errors = result.Errors });
        })
        .WithName("UpdateTrialRequestStatus")
        .WithSummary("Update trial request status");

        return app;
    }
}
