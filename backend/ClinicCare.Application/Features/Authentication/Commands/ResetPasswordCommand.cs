using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Authentication.Commands;

public class ResetPasswordCommand : IRequest<Result<ResetPasswordResponse>>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Optional: User ID to reset password for (for admin use)
    /// If not provided, uses Email to find user
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Optional: Specify if this is a SystemUser (Global admin)
    /// If not specified, will check both SystemUsers and Tenant Users
    /// </summary>
    public bool? IsSystemUser { get; set; }
}

public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty; // "SystemUser" or "TenantUser"
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IGlobalDbContext _globalContext;
    private readonly ITenantService _tenantService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        IGlobalDbContext globalContext,
        ITenantService tenantService,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _globalContext = globalContext;
        _tenantService = tenantService;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ResetPasswordResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAuthenticated)
            {
                return Result<ResetPasswordResponse>.Failure("User not authenticated");
            }

            var currentUserRole = _currentUserService.Role;
            if (!currentUserRole.HasValue)
            {
                return Result<ResetPasswordResponse>.Failure("User role not found");
            }

            // Check if user has permission to reset passwords
            var canResetPassword = currentUserRole.Value == UserRole.SuperAdmin || 
                                  currentUserRole.Value == UserRole.Admin;
            
            if (!canResetPassword)
            {
                return Result<ResetPasswordResponse>.Failure("You do not have permission to reset passwords");
            }

            // Determine if we're resetting a SystemUser or Tenant User
            bool isSystemUser = request.IsSystemUser ?? false;
            
            // If not specified, check SystemUsers first
            if (!request.IsSystemUser.HasValue)
            {
                var systemUser = await _globalContext.SystemUsers
                    .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
                
                if (systemUser != null)
                {
                    isSystemUser = true;
                }
            }

            if (isSystemUser)
            {
                // Reset password for SystemUser (Global admin)
                if (currentUserRole.Value != UserRole.SuperAdmin)
                {
                    return Result<ResetPasswordResponse>.Failure("Only SuperAdmin can reset SystemUser passwords");
                }

                Domain.Entities.SystemUser? systemUser = null;
                
                if (request.UserId.HasValue)
                {
                    systemUser = await _globalContext.SystemUsers
                        .FirstOrDefaultAsync(x => x.Id == request.UserId.Value, cancellationToken);
                }
                else
                {
                    systemUser = await _globalContext.SystemUsers
                        .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
                }

                if (systemUser == null)
                {
                    return Result<ResetPasswordResponse>.Failure("SystemUser not found");
                }

                // Update password
                systemUser.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                systemUser.UpdatedAt = DateTime.UtcNow;
                
                await _globalContext.SaveChangesAsync(cancellationToken);

                return Result<ResetPasswordResponse>.Success(new ResetPasswordResponse
                {
                    Success = true,
                    Message = "Password reset successfully",
                    Email = systemUser.Email,
                    UserType = "SystemUser"
                });
            }
            else
            {
                // Reset password for Tenant User
                int organizationId;
                Domain.Entities.User? user = null;

                // SuperAdmin can reset any tenant user
                // Admin (OrganizationAdmin) can only reset users in their organization
                if (currentUserRole.Value == UserRole.SuperAdmin)
                {
                    // SuperAdmin: Find user across all organizations
                    if (request.UserId.HasValue)
                    {
                        user = await _context.Users
                            .Include(x => x.Organization)
                            .FirstOrDefaultAsync(x => x.Id == request.UserId.Value && x.IsActive, cancellationToken);
                    }
                    else
                    {
                        user = await _context.Users
                            .Include(x => x.Organization)
                            .FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive, cancellationToken);
                    }
                }
                else if (currentUserRole.Value == UserRole.Admin)
                {
                    // Admin (OrganizationAdmin): Only users in their organization
                    organizationId = _currentUserService.OrganizationId ?? 0;
                    if (organizationId == 0)
                    {
                        return Result<ResetPasswordResponse>.Failure("Organization ID not found for current user");
                    }
                    
                    if (request.UserId.HasValue)
                    {
                        user = await _context.Users
                            .Include(x => x.Organization)
                            .FirstOrDefaultAsync(x => x.Id == request.UserId.Value && 
                                                     x.OrganizationId == organizationId && 
                                                     x.IsActive, cancellationToken);
                    }
                    else
                    {
                        user = await _context.Users
                            .Include(x => x.Organization)
                            .FirstOrDefaultAsync(x => x.Email == request.Email && 
                                                     x.OrganizationId == organizationId && 
                                                     x.IsActive, cancellationToken);
                    }
                }

                if (user == null)
                {
                    return Result<ResetPasswordResponse>.Failure("User not found or you don't have permission to reset this user's password");
                }

                // Update password
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
        }
        catch (Exception ex)
        {
            return Result<ResetPasswordResponse>.Failure($"Failed to reset password: {ex.Message}");
        }
    }
}

