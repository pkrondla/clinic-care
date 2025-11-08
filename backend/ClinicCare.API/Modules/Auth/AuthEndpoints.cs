using ClinicCare.Application.Features.Authentication.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicCare.API.Modules.Auth;

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
            [FromBody] LogoutCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            
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

        return endpoints;
    }
}
