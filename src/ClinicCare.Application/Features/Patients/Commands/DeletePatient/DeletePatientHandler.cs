using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Patients.Commands.DeletePatient;

public class DeletePatientHandler : IRequestHandler<DeletePatientCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public DeletePatientHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<bool>> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();

        var patient = await _context.Patients
            .Include(p => p.User)
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.OrganizationId == organizationId, cancellationToken);

        if (patient == null)
        {
            return Result<bool>.Failure("Patient not found.");
        }

        // Check if patient has any active appointments
        var hasActiveAppointments = await _context.Appointments
            .AnyAsync(a => a.PatientId == request.Id && 
                          a.Status != Domain.Enums.AppointmentStatus.Completed &&
                          a.Status != Domain.Enums.AppointmentStatus.Cancelled, cancellationToken);

        if (hasActiveAppointments)
        {
            return Result<bool>.Failure("Cannot delete patient with active appointments. Please cancel or complete all appointments first.");
        }

        // Soft delete patient and user
        patient.IsActive = false;
        patient.UpdatedAt = DateTime.UtcNow;
        patient.User.IsActive = false;
        patient.User.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

