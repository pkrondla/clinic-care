using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Authentication.Commands;

public class ResetPasswordCommand : IRequest<Result<ResetPasswordResponse>>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; } = string.Empty;

    public int? UserId { get; set; }
}

public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = "TenantUser";
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ResetPasswordResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAuthenticated)
                return Result<ResetPasswordResponse>.Failure("User not authenticated");

            var currentUserRole = _currentUserService.Role;
            if (!currentUserRole.HasValue)
                return Result<ResetPasswordResponse>.Failure("User role not found");

            if (currentUserRole.Value is not (UserRole.SuperAdmin or UserRole.Admin))
                return Result<ResetPasswordResponse>.Failure("You do not have permission to reset passwords");

            Domain.Entities.User? user;
            if (currentUserRole.Value == UserRole.SuperAdmin)
            {
                user = request.UserId.HasValue
                    ? await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId.Value && x.IsActive, cancellationToken)
                    : await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive, cancellationToken);
            }
            else
            {
                var organizationId = _currentUserService.OrganizationId ?? 0;
                if (organizationId == 0)
                    return Result<ResetPasswordResponse>.Failure("Organization ID not found for current user");

                user = request.UserId.HasValue
                    ? await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId.Value && x.OrganizationId == organizationId && x.IsActive, cancellationToken)
                    : await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email && x.OrganizationId == organizationId && x.IsActive, cancellationToken);
            }

            if (user == null)
                return Result<ResetPasswordResponse>.Failure("User not found or you don't have permission to reset this user's password");

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ResetPasswordResponse>.Success(new ResetPasswordResponse
            {
                Success = true,
                Message = "Password reset successfully",
                Email = user.Email,
                UserType = "TenantUser"
            });
        }
        catch (Exception ex)
        {
            return Result<ResetPasswordResponse>.Failure($"Failed to reset password: {ex.Message}");
        }
    }
}
