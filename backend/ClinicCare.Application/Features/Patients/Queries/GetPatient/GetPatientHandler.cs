using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Patients.Queries.GetPatient;

public class GetPatientHandler : IRequestHandler<GetPatientQuery, Result<PatientDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetPatientHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<PatientDetailDto>> Handle(GetPatientQuery request, CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();

        var patient = await _context.Patients
            .Include(p => p.User)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Clinic)
            .Include(p => p.Consultations)
                .ThenInclude(c => c.Doctor)
                    .ThenInclude(d => d.User)
            .Include(p => p.Consultations)
                .ThenInclude(c => c.Prescriptions)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.OrganizationId == organizationId, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDetailDto>.Failure("Patient not found.");
        }

        // Calculate statistics
        var totalAppointments = patient.Appointments.Count;
        var completedAppointments = patient.Appointments.Count(a => a.Status == Domain.Enums.AppointmentStatus.Completed);
        var cancelledAppointments = patient.Appointments.Count(a => a.Status == Domain.Enums.AppointmentStatus.Cancelled);
        var lastVisitDate = patient.Appointments
            .Where(a => a.Status == Domain.Enums.AppointmentStatus.Completed)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => a.AppointmentDate.ToDateTime(TimeOnly.MinValue))
            .FirstOrDefault();
        var firstVisitDate = patient.Appointments
            .OrderBy(a => a.AppointmentDate)
            .Select(a => a.AppointmentDate.ToDateTime(TimeOnly.MinValue))
            .FirstOrDefault();

        // Get recent appointments (last 10)
        var recentAppointments = patient.Appointments
            .OrderByDescending(a => a.AppointmentDate)
            .Take(10)
            .Select(a => new RecentAppointmentDto
            {
                Id = a.Id,
                TokenNumber = a.TokenNumber,
                AppointmentDate = a.AppointmentDate.ToDateTime(TimeOnly.MinValue),
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                DoctorName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                ClinicName = a.Clinic.Name,
                Notes = a.Notes
            })
            .ToList();

        // Get recent consultations (last 10)
        var recentConsultations = patient.Consultations
            .OrderByDescending(c => c.ConsultationDate)
            .Take(10)
            .Select(c => new RecentConsultationDto
            {
                Id = c.Id,
                ConsultationDate = c.ConsultationDate,
                ChiefComplaint = c.ChiefComplaint,
                Diagnosis = c.Diagnosis,
                DoctorName = $"{c.Doctor.User.FirstName} {c.Doctor.User.LastName}",
                ClinicName = c.Appointment.Clinic.Name,
                HasPrescription = c.Prescriptions.Any()
            })
            .ToList();

        var patientDto = new PatientDetailDto
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
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            IsActive = patient.IsActive,
            TotalAppointments = totalAppointments,
            TotalConsultations = patient.Consultations.Count,
            CompletedAppointments = completedAppointments,
            CancelledAppointments = cancelledAppointments,
            LastVisitDate = lastVisitDate,
            FirstVisitDate = firstVisitDate,
            RecentAppointments = recentAppointments,
            RecentConsultations = recentConsultations
        };

        return Result<PatientDetailDto>.Success(patientDto);
    }
}
