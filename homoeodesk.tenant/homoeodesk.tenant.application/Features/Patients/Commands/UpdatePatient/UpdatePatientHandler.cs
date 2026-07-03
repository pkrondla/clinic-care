using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public UpdatePatientHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();

        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.OrganizationId == organizationId, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDto>.Failure("Patient not found.");
        }

        // Check if email is being changed and if it's already taken
        if (patient.User.Email != request.Email)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && 
                                        u.OrganizationId == organizationId && 
                                        u.Id != patient.UserId, cancellationToken);

            if (existingUser != null)
            {
                return Result<PatientDto>.Failure("A user with this email already exists.");
            }
        }

        // Update user information
        patient.User.Email = request.Email;
        patient.User.FirstName = request.FirstName;
        patient.User.LastName = request.LastName;
        patient.User.Phone = request.Phone;
        patient.User.UpdatedAt = DateTime.UtcNow;

        // Update patient information
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;
        patient.BloodGroup = request.BloodGroup;
        patient.Address = request.Address;
        patient.EmergencyContact = request.EmergencyContact;
        patient.MedicalHistory = request.MedicalHistory;
        patient.PhotoUrl = request.PhotoUrl;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var patientDto = new PatientDto
        {
            Id = patient.Id,
            UserId = patient.UserId,
            PatientCode = patient.PatientCode,
            Email = patient.User.Email,
            FirstName = patient.User.FirstName,
            LastName = patient.User.LastName,
            FullName = $"{patient.User.FirstName} {patient.User.LastName}",
            Phone = patient.User.Phone,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.Age,
            Gender = patient.Gender,
            BloodGroup = patient.BloodGroup,
            Address = patient.Address,
            EmergencyContact = patient.EmergencyContact,
            MedicalHistory = patient.MedicalHistory,
            PhotoUrl = patient.PhotoUrl,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            IsActive = patient.IsActive
        };

        return Result<PatientDto>.Success(patientDto);
    }
}

