using ClinicCare.Application.Features.Organizations.Commands.CreateOrganization;
using ClinicCare.Application.Features.Organizations.Commands.UpdateOrganization;
using ClinicCare.Application.Features.Organizations.Queries.GetOrganization;
using ClinicCare.Application.Features.Organizations.Queries.GetOrganizations;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ClinicCare.API.Modules.Global;

public static class OrganizationsEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/global/organizations")
            .WithTags("Global Organizations")
            .WithOpenApi()
            .RequireAuthorization(); // TODO: Add SuperAdmin policy

        // Get all organizations
        group.MapGet("/", GetOrganizations)
            .WithName("GetOrganizations")
            .WithSummary("Get all organizations (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Get organization by ID
        group.MapGet("/{id:int}", GetOrganizationById)
            .WithName("GetOrganizationById")
            .WithSummary("Get organization by ID (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Get organization by subdomain
        group.MapGet("/subdomain/{subdomain}", GetOrganizationBySubdomain)
            .WithName("GetOrganizationBySubdomain")
            .WithSummary("Get organization by subdomain")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound);

        // Create new organization
        group.MapPost("/", CreateOrganization)
            .WithName("CreateOrganization")
            .WithSummary("Create a new organization and tenant database (Super Admin only)")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        // Update organization
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
        var query = new GetOrganizationsQuery();
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetOrganizationById(IMediator mediator, int id)
    {
        var query = new GetOrganizationQuery { Id = id };
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> GetOrganizationBySubdomain(IMediator mediator, string subdomain)
    {
        var query = new GetOrganizationQuery { Subdomain = subdomain };
        var result = await mediator.Send(query);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data })
            : Results.NotFound(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> CreateOrganization(
        IMediator mediator,
        CreateOrganizationCommand command)
    {
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Organization created successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UpdateOrganization(
        IMediator mediator,
        int id,
        UpdateOrganizationCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);

        return result.Succeeded
            ? Results.Ok(new { success = true, data = result.Data, message = "Organization updated successfully" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}

