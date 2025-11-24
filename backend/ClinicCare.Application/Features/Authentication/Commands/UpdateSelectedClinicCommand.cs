using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Authentication.Commands;

public class UpdateSelectedClinicCommand : IRequest<Result<UpdateSelectedClinicResponse>>
{
    public int ClinicId { get; set; }
}

public class UpdateSelectedClinicResponse
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class UpdateSelectedClinicCommandHandler : IRequestHandler<UpdateSelectedClinicCommand, Result<UpdateSelectedClinicResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSelectedClinicCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UpdateSelectedClinicResponse>> Handle(UpdateSelectedClinicCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result<UpdateSelectedClinicResponse>.Failure("User not authenticated");
        }

        // Get user
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result<UpdateSelectedClinicResponse>.Failure("User not found");
        }

        // Verify user has access to this clinic
        var hasAccess = await _context.UserClinicAccess
            .AnyAsync(x => x.UserId == userId && x.ClinicId == request.ClinicId && x.CanAccess && x.IsActive, cancellationToken);

        // Get clinic to verify it exists and user has access
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(x => x.Id == request.ClinicId && x.OrganizationId == user.OrganizationId && x.IsActive, cancellationToken);

        if (clinic == null)
        {
            return Result<UpdateSelectedClinicResponse>.Failure("Clinic not found or you don't have access to it");
        }

        // If user has explicit access mapping, verify it
        if (!hasAccess && await _context.UserClinicAccess.AnyAsync(x => x.UserId == userId, cancellationToken))
        {
            // User has some clinic mappings, so they must have access to this specific clinic
            return Result<UpdateSelectedClinicResponse>.Failure("You don't have access to this clinic");
        }

        // Update user's selected clinic
        user.SelectedClinicId = request.ClinicId;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<UpdateSelectedClinicResponse>.Success(new UpdateSelectedClinicResponse
        {
            ClinicId = request.ClinicId,
            ClinicName = clinic.Name,
            Message = "Clinic selected successfully"
        });
    }
}

