using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Authentication.Commands;

public class UpdateSelectedBranchCommand : IRequest<Result<UpdateSelectedBranchResponse>>
{
    public int BranchId { get; set; }
}

public class UpdateSelectedBranchResponse
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class UpdateSelectedBranchCommandHandler : IRequestHandler<UpdateSelectedBranchCommand, Result<UpdateSelectedBranchResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSelectedBranchCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UpdateSelectedBranchResponse>> Handle(UpdateSelectedBranchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result<UpdateSelectedBranchResponse>.Failure("User not authenticated");
        }

        // Get user
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result<UpdateSelectedBranchResponse>.Failure("User not found");
        }

        // Verify user has access to this branch
        var hasAccess = await _context.UserBranchAccess
            .AnyAsync(x => x.UserId == userId && x.BranchId == request.BranchId && x.CanAccess && x.IsActive, cancellationToken);

        // Get clinic to verify it exists and user has access
        var clinic = await _context.Branches
            .FirstOrDefaultAsync(x => x.Id == request.BranchId && x.OrganizationId == user.OrganizationId && x.IsActive, cancellationToken);

        if (clinic == null)
        {
            return Result<UpdateSelectedBranchResponse>.Failure("Branch not found or you don't have access to it");
        }

        // If user has explicit access mapping, verify it
        if (!hasAccess && await _context.UserBranchAccess.AnyAsync(x => x.UserId == userId, cancellationToken))
        {
            // User has some clinic mappings, so they must have access to this specific clinic
            return Result<UpdateSelectedBranchResponse>.Failure("You don't have access to this branch");
        }

        // Update user's selected clinic
        user.SelectedBranchId = request.BranchId;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<UpdateSelectedBranchResponse>.Success(new UpdateSelectedBranchResponse
        {
            BranchId = request.BranchId,
            BranchName = clinic.Name,
            Message = "Branch selected successfully"
        });
    }
}

