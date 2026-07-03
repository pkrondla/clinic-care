using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Doctors.Commands.DeleteDoctorAvailability;

public class DeleteDoctorAvailabilityHandler : IRequestHandler<DeleteDoctorAvailabilityCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteDoctorAvailabilityHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(DeleteDoctorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<Unit>.Failure("User not associated with any organization");
            }

            var availability = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(da => da.Id == request.Id 
                    && da.OrganizationId == organizationId.Value, cancellationToken);

            if (availability == null)
            {
                return Result<Unit>.Failure("Doctor availability not found");
            }

            // Check if there are any appointments scheduled for this availability
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.DoctorId == availability.DoctorId
                    && a.BranchId == availability.BranchId
                    && a.AppointmentDate.Value == availability.AvailableDate
                    && a.IsActive
                    && a.Status != Domain.Enums.AppointmentStatus.Cancelled, cancellationToken);

            if (hasAppointments)
            {
                // Soft delete instead of hard delete
                availability.IsActive = false;
                availability.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Hard delete if no appointments
                _context.DoctorAvailabilities.Remove(availability);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to delete doctor availability: {ex.Message}");
        }
    }
}

