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
                .ThenInclude(c => c.Appointment)
                    .ThenInclude(a => a.Clinic)
            .Include(p => p.Consultations)
                .ThenInclude(c => c.Prescriptions)
            .Include(p => p.Consultations)
                .ThenInclude(c => c.Photos)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.OrganizationId == organizationId, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDetailDto>.Failure("Patient not found.");
        }

        if (patient.User == null)
        {
            return Result<PatientDetailDto>.Failure("Patient user information not found.");
        }

        // Calculate statistics - materialize collections before ordering
        var appointments = patient.Appointments.ToList();
        var totalAppointments = appointments.Count;
        var completedAppointments = appointments.Count(a => a.Status == Domain.Enums.AppointmentStatus.Completed);
        var cancelledAppointments = appointments.Count(a => a.Status == Domain.Enums.AppointmentStatus.Cancelled);
        
        // Get last visit date - filter and convert to DateTime before ordering
        var completedAppointmentsWithDate = appointments
            .Where(a => a.Status == Domain.Enums.AppointmentStatus.Completed && a.AppointmentDate != null)
            .Select(a => a.AppointmentDate.Value.ToDateTime(TimeOnly.MinValue))
            .OrderByDescending(d => d)
            .FirstOrDefault();
        var lastVisitDate = completedAppointmentsWithDate;
        
        // Get first visit date - filter and convert to DateTime before ordering
        var appointmentsWithDate = appointments
            .Where(a => a.AppointmentDate != null)
            .Select(a => a.AppointmentDate.Value.ToDateTime(TimeOnly.MinValue))
            .OrderBy(d => d)
            .FirstOrDefault();
        var firstVisitDate = appointmentsWithDate;

        // Get recent appointments (last 10) - materialize before ordering
        var recentAppointments = appointments
            .Where(a => a.AppointmentDate != null)
            .OrderByDescending(a => a.AppointmentDate.Value)
            .Take(10)
            .Select(a => new RecentAppointmentDto
            {
                Id = a.Id,
                TokenNumber = a.TokenNumber,
                AppointmentDate = a.AppointmentDate.Value.ToDateTime(TimeOnly.MinValue),
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                DoctorName = a.Doctor?.User != null 
                    ? $"{a.Doctor.User.FirstName ?? string.Empty} {a.Doctor.User.LastName ?? string.Empty}".Trim()
                    : "Unknown",
                ClinicName = a.Clinic?.Name ?? "Unknown",
                Notes = a.Notes
            })
            .ToList();

        // Get recent consultations (last 10)
        var consultations = patient.Consultations.ToList();
        var recentConsultations = consultations
            .OrderByDescending(c => c.ConsultationDate)
            .Take(10)
            .Select(c => new RecentConsultationDto
            {
                Id = c.Id,
                ConsultationDate = c.ConsultationDate,
                ChiefComplaint = c.ChiefComplaint ?? string.Empty,
                Diagnosis = c.Diagnosis ?? string.Empty,
                DoctorName = c.Doctor?.User != null 
                    ? $"{c.Doctor.User.FirstName ?? string.Empty} {c.Doctor.User.LastName ?? string.Empty}".Trim()
                    : "Unknown",
                ClinicName = c.Appointment?.Clinic != null 
                    ? c.Appointment.Clinic.Name ?? "Unknown"
                    : "Unknown",
                HasPrescription = c.Prescriptions?.Any() ?? false,
                Photos = c.Photos?
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.DisplayOrder)
                    .Select(p => new ConsultationPhotoSummaryDto
                    {
                        Id = p.Id,
                        PhotoUrl = p.PhotoUrl,
                        Description = p.Description
                    })
                    .ToList()
            })
            .ToList();

        var patientDto = new PatientDetailDto
        {
            Id = patient.Id,
            UserId = patient.UserId,
            PatientCode = patient.PatientCode ?? string.Empty,
            Email = patient.User.Email ?? string.Empty,
            FirstName = patient.User.FirstName ?? string.Empty,
            LastName = patient.User.LastName ?? string.Empty,
            FullName = $"{patient.User.FirstName ?? string.Empty} {patient.User.LastName ?? string.Empty}".Trim(),
            Phone = patient.User.Phone ?? string.Empty,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.Age,
            Gender = patient.Gender ?? string.Empty,
            BloodGroup = patient.BloodGroup ?? string.Empty,
            Address = patient.Address ?? string.Empty,
            EmergencyContact = patient.EmergencyContact ?? string.Empty,
            MedicalHistory = patient.MedicalHistory ?? string.Empty,
            PhotoUrl = patient.PhotoUrl,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            IsActive = patient.IsActive,
            TotalAppointments = totalAppointments,
            TotalConsultations = consultations.Count,
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
