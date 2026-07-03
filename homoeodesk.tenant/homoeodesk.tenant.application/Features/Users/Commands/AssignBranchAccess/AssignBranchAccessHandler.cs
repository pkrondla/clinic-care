using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Users.Commands.AssignBranchAccess;

public class AssignBranchAccessHandler : IRequestHandler<AssignBranchAccessCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AssignBranchAccessHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(AssignBranchAccessCommand request, CancellationToken cancellationToken)
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

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.OrganizationId == organizationId.Value, cancellationToken);

            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // Validate clinic IDs
            if (request.BranchIds.Any())
            {
                var validBranches = await _context.Branches
                    .Where(c => request.BranchIds.Contains(c.Id) && c.OrganizationId == organizationId.Value && c.IsActive)
                    .CountAsync(cancellationToken);

                if (validBranches != request.BranchIds.Count)
                {
                    return Result<bool>.Failure("One or more clinic IDs are invalid.");
                }
            }

            // Remove existing clinic access
            var existingAccess = await _context.UserBranchAccess
                .Where(uca => uca.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            _context.UserBranchAccess.RemoveRange(existingAccess);

            // Add new clinic access
            if (request.BranchIds.Any())
            {
                foreach (var BranchId in request.BranchIds)
                {
                    var clinicAccess = new UserBranchAccess
                    {
                        UserId = request.UserId,
                        BranchId = BranchId,
                        CanAccess = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserBranchAccess.Add(clinicAccess);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to assign clinic access: {ex.Message}");
        }
    }
}

