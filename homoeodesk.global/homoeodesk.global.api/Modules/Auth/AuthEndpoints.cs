using HomoeoDesk.Global.Application.Features.Authentication.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomoeoDesk.Global.Api.Modules.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", async (
            [FromBody] LoginCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.Succeeded
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { message = "Login failed", errors = result.Errors });
        })
        .WithName("GlobalLogin")
        .WithSummary("Authenticate platform admin (SuperAdmin)");

        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.Succeeded
                ? Results.Ok(result.Data)
                : Results.Unauthorized();
        })
        .WithName("GlobalRefreshToken");

        group.MapPost("/logout", async (
            [FromBody] LogoutCommand? command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command ?? new LogoutCommand(), cancellationToken);
            return result.Succeeded
                ? Results.Ok(new { message = "Logged out successfully" })
                : Results.BadRequest(new { errors = result.Errors });
        })
        .WithName("GlobalLogout");

        return endpoints;
    }
}
