using HomoeoDesk.Tenant.Application.Features.Authentication.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomoeoDesk.Tenant.Api.Modules.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // Login endpoint
        group.MapPost("/login", async (
            [FromBody] LoginCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            
            if (result.Succeeded)
            {
                return Results.Ok(result.Data);
            }
            
            return Results.BadRequest(result.Errors);
        })
        .WithName("Login")
        .WithSummary("Authenticate user and return JWT token")
        .WithDescription("Authenticates a user with email and password, returns JWT token and user information")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // Refresh token endpoint
        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            
            if (result.Succeeded)
            {
                return Results.Ok(result.Data);
            }
            
            return Results.BadRequest(result.Errors);
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh JWT token")
        .WithDescription("Refreshes an expired JWT token using a valid refresh token")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // Logout endpoint
        group.MapPost("/logout", async (
            [FromBody] LogoutCommand? command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            // Create command if not provided (allows logout without body)
            var logoutCommand = command ?? new LogoutCommand();
            var result = await mediator.Send(logoutCommand, cancellationToken);
            
            if (result.Succeeded)
            {
                return Results.Ok(new { message = "Logged out successfully" });
            }
            
            return Results.BadRequest(result.Errors);
        })
        .WithName("Logout")
        .WithSummary("Logout user")
        .WithDescription("Logs out a user and invalidates their refresh token")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        // Reset password endpoint
        group.MapPost("/reset-password", async (
            [FromBody] ResetPasswordCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            
            if (result.Succeeded)
            {
                return Results.Ok(result.Data);
            }
            
            return Results.BadRequest(result.Errors);
        })
        .WithName("ResetPassword")
        .WithSummary("Reset user password")
        .WithDescription("Resets a user's password. SuperAdmin can reset any user's password. OrganizationAdmin can reset passwords for users in their organization.")
        .Produces<ResetPasswordResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization();

        // Update selected branch endpoint
        group.MapPost("/select-branch", SelectBranchHandler)
            .WithName("SelectBranch")
            .WithSummary("Update user's selected branch")
            .WithDescription("Updates the currently selected branch for the authenticated user. All transactions will be associated with this branch.")
            .Produces<UpdateSelectedBranchResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        // Legacy alias during migration
        group.MapPost("/select-clinic", SelectBranchHandler)
            .WithName("SelectClinic")
            .WithSummary("Update user's selected branch (legacy route)")
            .Produces<UpdateSelectedBranchResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> SelectBranchHandler(
        [FromBody] UpdateSelectedBranchCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (result.Succeeded)
        {
            return Results.Ok(result.Data);
        }

        return Results.BadRequest(result.Errors);
    }
}
