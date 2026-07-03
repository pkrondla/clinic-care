using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctorAvailability;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Doctors.Commands.CreateDoctorAvailability;

public class CreateDoctorAvailabilityHandler : IRequestHandler<CreateDoctorAvailabilityCommand, Result<DoctorAvailabilityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateDoctorAvailabilityHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DoctorAvailabilityDto>> Handle(CreateDoctorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<DoctorAvailabilityDto>.Failure("User not associated with any organization");
            }

            // Validate doctor exists and belongs to organization
            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId 
                    && d.OrganizationId == organizationId.Value 
                    && d.IsActive, cancellationToken);

            if (doctor == null)
            {
                return Result<DoctorAvailabilityDto>.Failure("Doctor not found or inactive");
            }

            // Validate clinic exists and belongs to organization
            var clinic = await _context.Branches
                .FirstOrDefaultAsync(c => c.Id == request.BranchId 
                    && c.OrganizationId == organizationId.Value 
                    && c.IsActive, cancellationToken);

            if (clinic == null)
            {
                return Result<DoctorAvailabilityDto>.Failure("Branch not found or inactive");
            }

            // Validate time range
            if (request.StartTime >= request.EndTime)
            {
                return Result<DoctorAvailabilityDto>.Failure("Start time must be before end time");
            }

            // Check for overlapping availability
            var overlapping = await _context.DoctorAvailabilities
                .AnyAsync(da => da.DoctorId == request.DoctorId
                    && da.BranchId == request.BranchId
                    && da.AvailableDate == request.AvailableDate
                    && da.IsActive
                    && ((da.StartTime <= request.StartTime && da.EndTime > request.StartTime)
                        || (da.StartTime < request.EndTime && da.EndTime >= request.EndTime)
                        || (da.StartTime >= request.StartTime && da.EndTime <= request.EndTime)), cancellationToken);

            if (overlapping)
            {
                return Result<DoctorAvailabilityDto>.Failure("Doctor already has availability scheduled for this time slot");
            }

            // Create availability
            var availability = new DoctorAvailability
            {
                OrganizationId = organizationId.Value,
                DoctorId = request.DoctorId,
                BranchId = request.BranchId,
                AvailableDate = request.AvailableDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.DoctorAvailabilities.Add(availability);
            await _context.SaveChangesAsync(cancellationToken);

            // Return DTO
            var dto = new DoctorAvailabilityDto
            {
                Id = availability.Id,
                DoctorId = availability.DoctorId,
                DoctorName = doctor.User.FirstName + " " + doctor.User.LastName,
                BranchId = availability.BranchId,
                BranchName = clinic.Name,
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
            return Result<DoctorAvailabilityDto>.Failure($"Failed to create doctor availability: {ex.Message}");
        }
    }
}

