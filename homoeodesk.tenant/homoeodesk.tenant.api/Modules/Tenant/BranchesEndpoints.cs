using HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch;
using HomoeoDesk.Tenant.Application.Features.Branches.Commands.UpdateBranch;
using HomoeoDesk.Tenant.Application.Features.Branches.Queries.GetBranch;
using HomoeoDesk.Tenant.Application.Features.Branches.Queries.GetBranches;
using MediatR;

namespace HomoeoDesk.Tenant.Api.Modules.Tenant;

public static class BranchesEndpoints
{
    public static IEndpointRouteBuilder MapBranchesEndpoints(this IEndpointRouteBuilder app)
    {
        MapBranchRoutes(app.MapGroup("/api/branches")
            .WithTags("Branches")
            .WithOpenApi()
            .RequireAuthorization());

        // Legacy alias during migration
        MapBranchRoutes(app.MapGroup("/api/clinics")
            .WithTags("Branches")
            .WithOpenApi()
            .RequireAuthorization());

        return app;
    }

    private static void MapBranchRoutes(RouteGroupBuilder group)
    {
        group.MapGet("/", GetBranches)
            .WithSummary("Get all branches for current organization")
            .Produces<object>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", GetBranch)
            .WithSummary("Get branch by ID")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBranch)
            .WithSummary("Create a new branch (Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", UpdateBranch)
            .WithSummary("Update branch (Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetBranches(IMediator mediator)
    {
        var query = new GetBranchesQuery();
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetBranch(IMediator mediator, int id)
    {
        var query = new GetBranchQuery { Id = id };
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateBranch(IMediator mediator, CreateBranchCommand command)
    {
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Branch created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UpdateBranch(IMediator mediator, int id, UpdateBranchCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Branch updated successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}
