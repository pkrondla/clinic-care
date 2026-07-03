using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.UpdateOrganization;
using HomoeoDesk.Global.Application.Features.Organizations.Queries.GetOrganization;
using HomoeoDesk.Global.Application.Features.Organizations.Queries.GetOrganizations;
using MediatR;

namespace HomoeoDesk.Global.Api.Modules.Global;

public static class OrganizationsEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/global/organizations")
            .WithTags("Global Organizations")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetOrganizations)
            .WithName("GetOrganizations")
            .WithSummary("Get all organizations (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", GetOrganizationById)
            .WithName("GetOrganizationById")
            .WithSummary("Get organization by ID (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        group.MapGet("/subdomain/{subdomain}", GetOrganizationBySubdomain)
            .WithName("GetOrganizationBySubdomain")
            .WithSummary("Get organization by subdomain")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateOrganization)
            .WithName("CreateOrganization")
            .WithSummary("Create a new organization and tenant database (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", UpdateOrganization)
            .WithName("UpdateOrganization")
            .WithSummary("Update an existing organization (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetOrganizations(IMediator mediator)
    {
        var result = await mediator.Send(new GetOrganizationsQuery());

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetOrganizationById(IMediator mediator, int id)
    {
        var result = await mediator.Send(new GetOrganizationQuery { Id = id });

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetOrganizationBySubdomain(IMediator mediator, string subdomain)
    {
        var result = await mediator.Send(new GetOrganizationQuery { Subdomain = subdomain });

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateOrganization(IMediator mediator, CreateOrganizationCommand command)
    {
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Organization created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UpdateOrganization(IMediator mediator, int id, UpdateOrganizationCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Organization updated successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}
