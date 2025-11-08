using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Authentication.Commands;

public class RefreshTokenCommand : IRequest<Result<LoginResponse>>
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate refresh token
            var user = await _context.Users
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && 
                                         u.RefreshTokenExpiryTime > DateTime.UtcNow, 
                                         cancellationToken);

            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid or expired refresh token");
            }

            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user with new refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry

            await _context.SaveChangesAsync(cancellationToken);

            // Get user's available clinics
            var availableClinics = await _context.Clinics
                .Where(c => c.OrganizationId == user.OrganizationId && c.IsActive)
                .Select(c => new ClinicInfo
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code
                })
                .ToListAsync(cancellationToken);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // 1 hour expiry for access token
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Role = user.Role,
                    OrganizationId = user.OrganizationId,
                    OrganizationName = user.Organization?.Name ?? string.Empty,
                    SelectedClinicId = user.SelectedClinicId,
                    SelectedClinicName = availableClinics.FirstOrDefault(c => c.Id == user.SelectedClinicId)?.Name
                },
                AvailableClinics = availableClinics
            };

            return Result<LoginResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<LoginResponse>.Failure($"An error occurred while refreshing token: {ex.Message}");
        }
    }
}

