using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Reports.Queries.GetPatientReport;

public class GetPatientReportHandler : IRequestHandler<GetPatientReportQuery, Result<PatientReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientReportHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PatientReportDto>> Handle(GetPatientReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<PatientReportDto>.Failure("User not associated with any organization");
            }

            // If PatientId is provided, get single patient report
            if (request.PatientId.HasValue)
            {
                return await GetSinglePatientReport(request.PatientId.Value, request, organizationId.Value, cancellationToken);
            }

            // Otherwise, get aggregated report for all patients matching filters
            return await GetAggregatedPatientReport(request, organizationId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<PatientReportDto>.Failure($"Failed to generate patient report: {ex.Message}");
        }
    }

    private async Task<Result<PatientReportDto>> GetSinglePatientReport(
        int patientId,
        GetPatientReportQuery request,
        int organizationId,
        CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == patientId 
                && p.OrganizationId == organizationId 
                && p.IsActive, cancellationToken);

        if (patient == null)
        {
            return Result<PatientReportDto>.Failure("Patient not found");
        }

        var startDate = request.StartDate?.Date ?? DateTime.MinValue;
        var endDate = request.EndDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        // Get appointments
        var appointmentsQuery = _context.Appointments
            .Include(a => a.Clinic)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Where(a => a.PatientId == patientId
                && a.OrganizationId == organizationId
                && a.IsActive);

        if (request.ClinicId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.ClinicId == request.ClinicId.Value);
        }

        if (request.DoctorId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == request.DoctorId.Value);
        }

        if (request.StartDate.HasValue)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate);
            appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate.Value >= startDateOnly);
        }

        if (request.EndDate.HasValue)
        {
            var endDateOnly = DateOnly.FromDateTime(request.EndDate.Value);
            appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate.Value <= endDateOnly);
        }

        var appointments = await appointmentsQuery
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);

        // Get consultations
        var consultationsQuery = _context.Consultations
            .Include(c => c.Doctor)
                .ThenInclude(d => d.User)
            .Include(c => c.Appointment)
                .ThenInclude(a => a!.Clinic)
            .Where(c => c.PatientId == patientId
                && c.OrganizationId == organizationId
                && c.IsActive);

        if (request.ClinicId.HasValue)
        {
            consultationsQuery = consultationsQuery.Where(c => 
                c.Appointment != null && c.Appointment.ClinicId == request.ClinicId.Value);
        }

        if (request.DoctorId.HasValue)
        {
            consultationsQuery = consultationsQuery.Where(c => c.DoctorId == request.DoctorId.Value);
        }

        if (request.StartDate.HasValue)
        {
            consultationsQuery = consultationsQuery.Where(c => c.ConsultationDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            consultationsQuery = consultationsQuery.Where(c => c.ConsultationDate <= request.EndDate.Value);
        }

        var consultations = await consultationsQuery
            .OrderByDescending(c => c.ConsultationDate)
            .ToListAsync(cancellationToken);

        // Get prescriptions
        var prescriptionsQuery = _context.Prescriptions
            .Include(p => p.Consultation)
                .ThenInclude(c => c!.Doctor)
                    .ThenInclude(d => d.User)
            .Include(p => p.PrescriptionItems)
                // Don't include Medicine navigation property - it's ignored in configuration
            .Where(p => p.Consultation != null 
                && p.Consultation.PatientId == patientId
                && p.OrganizationId == organizationId
                && p.IsActive);

        if (request.ClinicId.HasValue)
        {
            prescriptionsQuery = prescriptionsQuery.Where(p => 
                p.Consultation != null 
                && p.Consultation.Appointment != null
                && p.Consultation.Appointment.ClinicId == request.ClinicId.Value);
        }

        if (request.DoctorId.HasValue)
        {
            prescriptionsQuery = prescriptionsQuery.Where(p => 
                p.Consultation != null 
                && p.Consultation.DoctorId == request.DoctorId.Value);
        }

        if (request.StartDate.HasValue)
        {
            prescriptionsQuery = prescriptionsQuery.Where(p => p.IssuedDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            prescriptionsQuery = prescriptionsQuery.Where(p => p.IssuedDate <= request.EndDate.Value);
        }

        var prescriptions = await prescriptionsQuery
            .OrderByDescending(p => p.IssuedDate)
            .ToListAsync(cancellationToken);

        // Get invoices
        var invoicesQuery = _context.Invoices
            .Where(i => i.PatientId == patientId
                && i.OrganizationId == organizationId
                && i.IsActive);

        if (request.ClinicId.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.ClinicId == request.ClinicId.Value);
        }

        if (request.StartDate.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.InvoiceDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.InvoiceDate <= request.EndDate.Value);
        }

        var invoices = await invoicesQuery
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);

        // Build visit history
        var visitHistory = appointments.Select(a => new PatientVisitDto
        {
            VisitDate = a.AppointmentDate?.Value.ToDateTime(TimeOnly.MinValue) ?? a.CreatedAt,
            ClinicName = a.Clinic?.Name ?? string.Empty,
            DoctorName = a.Doctor?.User != null 
                ? a.Doctor.User.FullName 
                : string.Empty,
            AppointmentType = a.Type.ToString(),
            Status = a.Status.ToString(),
            ConsultationId = consultations.FirstOrDefault(c => c.AppointmentId == a.Id)?.Id,
            PrescriptionId = prescriptions.FirstOrDefault(p => 
                p.Consultation != null && p.Consultation.AppointmentId == a.Id)?.Id,
            InvoiceId = invoices.FirstOrDefault(i => 
                i.Prescription != null 
                && prescriptions.Any(p => p.Id == i.PrescriptionId 
                    && p.Consultation != null 
                    && p.Consultation.AppointmentId == a.Id))?.Id
        }).ToList();

        // Build treatment summary
        var treatmentSummary = consultations.Select(c => new TreatmentSummaryDto
        {
            ConsultationDate = c.ConsultationDate,
            DoctorName = c.Doctor?.User != null 
                ? c.Doctor.User.FullName 
                : string.Empty,
            Diagnosis = c.Diagnosis ?? string.Empty,
            TreatmentPlan = c.TreatmentPlan ?? string.Empty,
            Notes = c.Notes ?? string.Empty
        }).ToList();

        // Build medication history
        var medicationHistory = prescriptions.Select(p => new MedicationHistoryDto
        {
            PrescriptionDate = p.IssuedDate,
            PrescriptionNumber = p.PrescriptionNumber,
            DoctorName = p.Consultation?.Doctor?.User != null
                ? p.Consultation.Doctor.User.FullName
                : string.Empty,
            MedicineCount = p.PrescriptionItems.Count,
            Status = p.Status.ToString(),
            Medications = p.PrescriptionItems.Select(pi => new MedicationItemDto
            {
                MedicineName = pi.MedicineName, // Use MedicineName directly from PrescriptionItem
                Dosage = pi.Dosage ?? string.Empty,
                Frequency = pi.Frequency ?? string.Empty,
                Duration = 0, // Duration is stored as string in entity, parse if needed
                DurationUnit = pi.Duration ?? string.Empty, // Use Duration string as DurationUnit
                Instructions = pi.Instructions ?? string.Empty
            }).ToList()
        }).ToList();

        // Build payment history
        var paymentHistory = invoices.Select(i => new PaymentHistoryDto
        {
            InvoiceDate = i.InvoiceDate,
            InvoiceNumber = i.InvoiceNumber,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            BalanceAmount = i.BalanceAmount,
            PaymentMethod = i.PaymentMethod ?? string.Empty,
            Status = i.Status.ToString(),
            PaymentDate = i.PaymentDate
        }).ToList();

        var report = new PatientReportDto
        {
            PatientId = patient.Id,
            PatientName = patient.User?.FullName ?? "Unknown",
            PatientCode = patient.PatientCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalVisits = appointments.Count,
            TotalConsultations = consultations.Count,
            TotalPrescriptions = prescriptions.Count,
            TotalInvoices = invoices.Count,
            TotalAmountPaid = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
            TotalAmountPending = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount),
            VisitHistory = visitHistory,
            TreatmentSummary = treatmentSummary,
            MedicationHistory = medicationHistory,
            PaymentHistory = paymentHistory
        };

        return Result<PatientReportDto>.Success(report);
    }

    private async Task<Result<PatientReportDto>> GetAggregatedPatientReport(
        GetPatientReportQuery request,
        int organizationId,
        CancellationToken cancellationToken)
    {
        // For aggregated reports, return summary statistics across all patients
        var startDate = request.StartDate?.Date ?? DateTime.MinValue;
        var endDate = request.EndDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        var appointmentsQuery = _context.Appointments
            .Where(a => a.OrganizationId == organizationId && a.IsActive);

        var consultationsQuery = _context.Consultations
            .Where(c => c.OrganizationId == organizationId && c.IsActive);

        var prescriptionsQuery = _context.Prescriptions
            .Where(p => p.OrganizationId == organizationId && p.IsActive);

        var invoicesQuery = _context.Invoices
            .Where(i => i.OrganizationId == organizationId && i.IsActive);

        if (request.ClinicId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.ClinicId == request.ClinicId.Value);
            consultationsQuery = consultationsQuery.Where(c => 
                c.Appointment != null && c.Appointment.ClinicId == request.ClinicId.Value);
            prescriptionsQuery = prescriptionsQuery.Where(p => 
                p.Consultation != null 
                && p.Consultation.Appointment != null
                && p.Consultation.Appointment.ClinicId == request.ClinicId.Value);
            invoicesQuery = invoicesQuery.Where(i => i.ClinicId == request.ClinicId.Value);
        }

        if (request.DoctorId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == request.DoctorId.Value);
            consultationsQuery = consultationsQuery.Where(c => c.DoctorId == request.DoctorId.Value);
            prescriptionsQuery = prescriptionsQuery.Where(p => 
                p.Consultation != null && p.Consultation.DoctorId == request.DoctorId.Value);
        }

        if (request.StartDate.HasValue)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate);
            appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate.Value >= startDateOnly);
            consultationsQuery = consultationsQuery.Where(c => c.ConsultationDate >= startDate);
            prescriptionsQuery = prescriptionsQuery.Where(p => p.IssuedDate >= startDate);
            invoicesQuery = invoicesQuery.Where(i => i.InvoiceDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDateOnly = DateOnly.FromDateTime(request.EndDate.Value);
            appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate.Value <= endDateOnly);
            consultationsQuery = consultationsQuery.Where(c => c.ConsultationDate <= request.EndDate.Value);
            prescriptionsQuery = prescriptionsQuery.Where(p => p.IssuedDate <= request.EndDate.Value);
            invoicesQuery = invoicesQuery.Where(i => i.InvoiceDate <= request.EndDate.Value);
        }

        var totalVisits = await appointmentsQuery.CountAsync(cancellationToken);
        var totalConsultations = await consultationsQuery.CountAsync(cancellationToken);
        var totalPrescriptions = await prescriptionsQuery.CountAsync(cancellationToken);
        var invoices = await invoicesQuery.ToListAsync(cancellationToken);

        var report = new PatientReportDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalVisits = totalVisits,
            TotalConsultations = totalConsultations,
            TotalPrescriptions = totalPrescriptions,
            TotalInvoices = invoices.Count,
            TotalAmountPaid = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
            TotalAmountPending = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount)
        };

        return Result<PatientReportDto>.Success(report);
    }
}

