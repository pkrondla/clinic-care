using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Authentication.Commands;

public class LogoutCommand : IRequest<Result<bool>>, ISkipTenantResolution
{
    public string? RefreshToken { get; set; }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public LogoutCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user from JWT token (if authenticated) or from refresh token
            var userId = _currentUserService.UserId;
            Domain.Entities.User? user = null;

            if (userId.HasValue)
            {
                // User is authenticated via JWT, get user by ID
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                // Fallback: find user by refresh token if provided
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);
            }

            if (user != null)
            {
                // Clear refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                await _context.SaveChangesAsync(cancellationToken);
            }

            // Always return success, even if user not found (token might already be cleared)
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"An error occurred while logging out: {ex.Message}");
        }
    }
}

