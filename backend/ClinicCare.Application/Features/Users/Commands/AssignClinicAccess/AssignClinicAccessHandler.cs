using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Users.Commands.AssignClinicAccess;

public class AssignClinicAccessHandler : IRequestHandler<AssignClinicAccessCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AssignClinicAccessHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(AssignClinicAccessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            var currentUserRole = _currentUserService.Role;
            var organizationId = _currentUserService.OrganizationId;

            if (!currentUserId.HasValue || !organizationId.HasValue)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            // Only Admin (OrganizationAdmin) can assign clinic access
            if (currentUserRole != UserRole.Admin)
            {
                return Result<bool>.Failure("Access denied. Only Organization Admin can assign clinic access.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.OrganizationId == organizationId.Value, cancellationToken);

            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // Validate clinic IDs
            if (request.ClinicIds.Any())
            {
                var validClinics = await _context.Clinics
                    .Where(c => request.ClinicIds.Contains(c.Id) && c.OrganizationId == organizationId.Value && c.IsActive)
                    .CountAsync(cancellationToken);

                if (validClinics != request.ClinicIds.Count)
                {
                    return Result<bool>.Failure("One or more clinic IDs are invalid.");
                }
            }

            // Remove existing clinic access
            var existingAccess = await _context.UserClinicAccess
                .Where(uca => uca.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            _context.UserClinicAccess.RemoveRange(existingAccess);

            // Add new clinic access
            if (request.ClinicIds.Any())
            {
                foreach (var clinicId in request.ClinicIds)
                {
                    var clinicAccess = new UserClinicAccess
                    {
                        UserId = request.UserId,
                        ClinicId = clinicId,
                        CanAccess = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserClinicAccess.Add(clinicAccess);
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

