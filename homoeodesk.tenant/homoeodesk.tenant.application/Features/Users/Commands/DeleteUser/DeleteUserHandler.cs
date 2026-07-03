using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUserHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureRole(UserRole.Admin);

        try
        {
            var currentUserId = _currentUserService.UserId;
            var organizationId = _currentUserService.OrganizationId;

            if (!currentUserId.HasValue || !organizationId.HasValue)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            // Cannot delete yourself
            if (currentUserId.Value == request.Id)
            {
                return Result<bool>.Failure("You cannot delete your own account.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id && u.OrganizationId == organizationId.Value, cancellationToken);

            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // Soft delete - set IsActive to false
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            // Also deactivate clinic access
            var clinicAccess = await _context.UserBranchAccess
                .Where(uca => uca.UserId == user.Id)
                .ToListAsync(cancellationToken);

            foreach (var access in clinicAccess)
            {
                access.IsActive = false;
                access.UpdatedAt = DateTime.UtcNow;
            }

            // Deactivate doctor profile if exists
            if (user.DoctorProfile != null)
            {
                user.DoctorProfile.IsActive = false;
                user.DoctorProfile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete user: {ex.Message}");
        }
    }
}

