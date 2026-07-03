using HomoeoDesk.Global.Application.Features.Subscriptions.Commands.CancelSubscription;
using HomoeoDesk.Global.Application.Features.Subscriptions.Commands.CreateSubscription;
using HomoeoDesk.Global.Application.Features.Subscriptions.Commands.UpdateSubscription;
using HomoeoDesk.Global.Application.Features.Subscriptions.Queries.GetSubscription;
using HomoeoDesk.Global.Application.Features.Subscriptions.Queries.GetSubscriptions;
using MediatR;

namespace HomoeoDesk.Global.Api.Modules.Global;

public static class SubscriptionsEndpoints
{
    public static IEndpointRouteBuilder MapSubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subscriptions")
            .WithTags("Subscriptions")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", async (
            IMediator mediator,
            int? organizationId,
            string? plan,
            string? status,
            string? search) =>
        {
            var result = await mediator.Send(new GetSubscriptionsQuery
            {
                OrganizationId = organizationId,
                Plan = plan,
                Status = status,
                Search = search
            });

            return result.Succeeded
                ? Results.Ok(new { success = true, data = result.Data })
                : Results.BadRequest(new { success = false, errors = result.Errors });
        })
        .WithName("GetSubscriptions");

        group.MapGet("/organization/{organizationId:int}", async (IMediator mediator, int organizationId) =>
        {
            var result = await mediator.Send(new GetSubscriptionQuery { OrganizationId = organizationId });

            return result.Succeeded
                ? Results.Ok(new { success = true, data = result.Data })
                : Results.NotFound(new { success = false, errors = result.Errors });
        })
        .WithName("GetOrganizationSubscription");

        group.MapGet("/{id:int}", async (IMediator mediator, int id) =>
        {
            var result = await mediator.Send(new GetSubscriptionQuery { Id = id });

            return result.Succeeded
                ? Results.Ok(new { success = true, data = result.Data })
                : Results.NotFound(new { success = false, errors = result.Errors });
        })
        .WithName("GetSubscriptionById");

        group.MapPost("/", async (IMediator mediator, CreateSubscriptionCommand command) =>
        {
            var result = await mediator.Send(command);

            return result.Succeeded
                ? Results.Ok(new { success = true, data = result.Data })
                : Results.BadRequest(new { success = false, errors = result.Errors });
        })
        .WithName("CreateSubscription");

        group.MapPut("/{id:int}", async (IMediator mediator, int id, UpdateSubscriptionCommand command) =>
        {
            command.Id = id;
            var result = await mediator.Send(command);

            return result.Succeeded
                ? Results.Ok(new { success = true, data = result.Data })
                : Results.BadRequest(new { success = false, errors = result.Errors });
        })
        .WithName("UpdateSubscription");

        group.MapPost("/{id:int}/cancel", async (IMediator mediator, int id, CancelSubscriptionCommand? command) =>
        {
            var payload = command ?? new CancelSubscriptionCommand();
            payload.Id = id;
            var result = await mediator.Send(payload);

            return result.Succeeded
                ? Results.Ok(new { success = true, message = "Subscription cancelled" })
                : Results.BadRequest(new { success = false, errors = result.Errors });
        })
        .WithName("CancelSubscription");

        return app;
    }
}
