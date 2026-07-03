using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctorAvailability;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Doctors.Commands.UpdateDoctorAvailability;

public class UpdateDoctorAvailabilityHandler : IRequestHandler<UpdateDoctorAvailabilityCommand, Result<DoctorAvailabilityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateDoctorAvailabilityHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DoctorAvailabilityDto>> Handle(UpdateDoctorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<DoctorAvailabilityDto>.Failure("User not associated with any organization");
            }

            var availability = await _context.DoctorAvailabilities
                .Include(da => da.Doctor)
                    .ThenInclude(d => d.User)
                .Include(da => da.Branch)
                .FirstOrDefaultAsync(da => da.Id == request.Id 
                    && da.OrganizationId == organizationId.Value, cancellationToken);

            if (availability == null)
            {
                return Result<DoctorAvailabilityDto>.Failure("Doctor availability not found");
            }

            // Validate time range
            if (request.StartTime >= request.EndTime)
            {
                return Result<DoctorAvailabilityDto>.Failure("Start time must be before end time");
            }

            // Check for overlapping availability (excluding current record)
            var overlapping = await _context.DoctorAvailabilities
                .AnyAsync(da => da.Id != request.Id
                    && da.DoctorId == availability.DoctorId
                    && da.BranchId == availability.BranchId
                    && da.AvailableDate == request.AvailableDate
                    && da.IsActive
                    && ((da.StartTime <= request.StartTime && da.EndTime > request.StartTime)
                        || (da.StartTime < request.EndTime && da.EndTime >= request.EndTime)
                        || (da.StartTime >= request.StartTime && da.EndTime <= request.EndTime)), cancellationToken);

            if (overlapping)
            {
                return Result<DoctorAvailabilityDto>.Failure("Doctor already has availability scheduled for this time slot");
            }

            // Update availability
            availability.AvailableDate = request.AvailableDate;
            availability.StartTime = request.StartTime;
            availability.EndTime = request.EndTime;
            availability.IsActive = request.IsActive;
            availability.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Return DTO
            var dto = new DoctorAvailabilityDto
            {
                Id = availability.Id,
                DoctorId = availability.DoctorId,
                DoctorName = availability.Doctor.User.FirstName + " " + availability.Doctor.User.LastName,
                BranchId = availability.BranchId,
                BranchName = availability.Branch.Name,
                AvailableDate = availability.AvailableDate,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                IsActive = availability.IsActive,
                CreatedAt = availability.CreatedAt,
                UpdatedAt = availability.UpdatedAt
            };

            return Result<DoctorAvailabilityDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DoctorAvailabilityDto>.Failure($"Failed to update doctor availability: {ex.Message}");
        }
    }
}

