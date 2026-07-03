using HomoeoDesk.Tenant.Application.Features.Users.Commands.AssignBranchAccess;
using HomoeoDesk.Tenant.Application.Features.Users.Commands.CreateUser;
using HomoeoDesk.Tenant.Application.Features.Users.Commands.DeleteUser;
using HomoeoDesk.Tenant.Application.Features.Users.Commands.UpdateUser;
using HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUser;
using HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomoeoDesk.Tenant.Api.Modules.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all users
        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get all users")
            .Produces<List<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get user by ID
        group.MapGet("/{id:int}", GetUser)
            .WithName("GetUser")
            .WithSummary("Get user by ID")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create user
        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update user
        group.MapPut("/{id:int}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update an existing user")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        // Delete user (soft delete)
        group.MapDelete("/{id:int}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Delete a user (soft delete)")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        // Assign clinic access
        group.MapPost("/{id:int}/clinic-access", AssignBranchAccess)
            .WithName("AssignBranchAccess")
            .WithSummary("Assign clinic access to a user")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> GetUsers(
        [AsParameters] GetUsersQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);

        if (result.Succeeded)
        {
            return Results.Ok(result.Data);
        }

        return Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> GetUser(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetUserQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);

        if (result.Succeeded)
        {
            return Results.Ok(result.Data);
        }

        if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
        {
            return Results.NotFound(result.Errors);
        }

        return Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> CreateUser(
        CreateUserCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (result.Succeeded)
        {
            return Results.Created($"/api/users/{result.Data?.Id}", result.Data);
        }

        return Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> UpdateUser(
        int id,
        UpdateUserCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken);

        if (result.Succeeded)
        {
            return Results.Ok(result.Data);
        }

        if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
        {
            return Results.NotFound(result.Errors);
        }

        return Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> DeleteUser(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);

        if (result.Succeeded)
        {
            return Results.Ok(result.Data);
        }

        if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
        {
            return Results.NotFound(result.Errors);
        }

        return Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> AssignBranchAccess(
        int id,
        AssignBranchAccessCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.UserId = id;
        var result = await mediator.Send(command, cancellationToken);

        if (result.Succeeded)
        {
            return Results.Ok(result.Data);
        }

        if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
        {
            return Results.NotFound(result.Errors);
        }

        return Results.BadRequest(result.Errors);
    }
}

