using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IPasswordHasher _passwordHasher;

    public CreatePatientHandler(
        IApplicationDbContext context,
        ITenantService tenantService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _tenantService = tenantService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<PatientDto>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();

        // Check if user with email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.OrganizationId == organizationId, cancellationToken);

        if (existingUser != null)
        {
            return Result<PatientDto>.Failure("A user with this email already exists.");
        }

        // Generate unique patient code
        var patientCode = await GeneratePatientCodeAsync(organizationId, cancellationToken);

        // Create user account
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Role = Domain.Enums.UserRole.Patient,
            OrganizationId = organizationId,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Create patient profile
        var patient = new Patient
        {
            UserId = user.Id,
            PatientCode = patientCode,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            BloodGroup = request.BloodGroup,
            Address = request.Address,
            EmergencyContact = request.EmergencyContact,
            MedicalHistory = request.MedicalHistory,
            OrganizationId = organizationId,
            IsActive = true
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        // Create user-organization relationship
        var userOrganization = new UserOrganization
        {
            UserId = user.Id,
            OrganizationId = organizationId,
            IsActive = true
        };

        _context.UserOrganizations.Add(userOrganization);
        await _context.SaveChangesAsync(cancellationToken);

        var patientDto = new PatientDto
        {
            Id = patient.Id,
            UserId = user.Id,
            PatientCode = patient.PatientCode,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            Phone = user.Phone,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.Age,
            Gender = patient.Gender,
            BloodGroup = patient.BloodGroup,
            Address = patient.Address,
            EmergencyContact = patient.EmergencyContact,
            MedicalHistory = patient.MedicalHistory,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            IsActive = patient.IsActive
        };

        return Result<PatientDto>.Success(patientDto);
    }

    private async Task<string> GeneratePatientCodeAsync(int organizationId, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var year = today.Year;
        var month = today.Month.ToString("D2");
        var day = today.Day.ToString("D2");

        // Get the count of patients created today for this organization
        var todayStart = new DateTime(today.Year, today.Month, today.Day);
        var todayEnd = todayStart.AddDays(1);

        var todayPatientCount = await _context.Patients
            .Where(p => p.OrganizationId == organizationId && 
                       p.CreatedAt >= todayStart && 
                       p.CreatedAt < todayEnd)
            .CountAsync(cancellationToken);

        var sequenceNumber = (todayPatientCount + 1).ToString("D4");
        
        return $"P{year}{month}{day}{sequenceNumber}";
    }
}

