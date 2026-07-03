using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;

public class CreateConsultationHandler : IRequestHandler<CreateConsultationCommand, Result<ConsultationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateConsultationHandler> _logger;
    private readonly INotificationService _notificationService;

    public CreateConsultationHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateConsultationHandler> logger,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<Result<ConsultationDto>> Handle(CreateConsultationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating consultation for AppointmentId: {AppointmentId}, PatientId: {PatientId}, DoctorId: {DoctorId}",
                request.AppointmentId, request.PatientId, request.DoctorId);

            var appointment = await _context.Appointments
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.User)
                .Include(x => x.Branch)
                .Include(x => x.Consultation)
                .FirstOrDefaultAsync(x => x.Id == request.AppointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found: {AppointmentId}", request.AppointmentId);
                return Result<ConsultationDto>.Failure(new[] { "Appointment not found." });
            }

            _logger.LogInformation("Appointment found: Id={Id}, PatientId={PatientId}, DoctorId={DoctorId}, OrganizationId={OrganizationId}",
                appointment.Id, appointment.PatientId, appointment.DoctorId, appointment.OrganizationId);

            if (appointment.PatientId != request.PatientId)
            {
                _logger.LogWarning("Patient ID mismatch. Appointment PatientId: {AppointmentPatientId}, Request PatientId: {RequestPatientId}",
                    appointment.PatientId, request.PatientId);
                return Result<ConsultationDto>.Failure(new[] { "Appointment does not belong to the specified patient." });
            }

            if (request.DoctorId > 0 && appointment.DoctorId != request.DoctorId)
            {
                _logger.LogWarning("Doctor ID mismatch. Appointment DoctorId: {AppointmentDoctorId}, Request DoctorId: {RequestDoctorId}",
                    appointment.DoctorId, request.DoctorId);
                return Result<ConsultationDto>.Failure(new[] { "Appointment does not belong to the specified doctor." });
            }

            var existing = await _context.Consultations
                .Include(c => c.Appointment)
                .Include(c => c.Prescriptions)
                .FirstOrDefaultAsync(c => c.AppointmentId == request.AppointmentId, cancellationToken);

            if (existing != null)
            {
                _logger.LogWarning("Consultation already exists for appointment: {AppointmentId}", request.AppointmentId);
                return Result<ConsultationDto>.Failure(new[] { "Consultation already exists for this appointment." });
            }

            var consultation = new Consultation
            {
                OrganizationId = appointment.OrganizationId,
                AppointmentId = request.AppointmentId,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                ConsultationDate = DateTime.UtcNow,
                ConsultationType = (int)appointment.Type,
                ChiefComplaint = request.ChiefComplaint,
                Symptoms = request.Symptoms ?? string.Empty,
                Examination = request.Examination ?? string.Empty,
                Diagnosis = request.Diagnosis ?? string.Empty,
                TreatmentPlan = request.TreatmentPlan ?? string.Empty,
                Notes = request.Notes ?? string.Empty,
                ConsultationFee = request.ConsultationFee
            };

            _logger.LogInformation("Adding consultation to database...");
            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Consultation created successfully with Id: {ConsultationId}", consultation.Id);

            var dto = new ConsultationDto
            {
                Id = consultation.Id,
                AppointmentId = consultation.AppointmentId,
                PatientId = consultation.PatientId,
                PatientName = appointment.Patient?.User != null
                    ? $"{appointment.Patient.User.FirstName ?? string.Empty} {appointment.Patient.User.LastName ?? string.Empty}".Trim()
                    : "Unknown",
                DoctorId = consultation.DoctorId,
                DoctorName = appointment.Doctor?.User != null
                    ? $"{appointment.Doctor.User.FirstName ?? string.Empty} {appointment.Doctor.User.LastName ?? string.Empty}".Trim()
                    : "Unknown",
                ConsultationDate = consultation.ConsultationDate,
                ChiefComplaint = consultation.ChiefComplaint,
                Symptoms = consultation.Symptoms,
                Examination = consultation.Examination,
                Diagnosis = consultation.Diagnosis,
                TreatmentPlan = consultation.TreatmentPlan,
                Notes = consultation.Notes,
                ConsultationFee = consultation.ConsultationFee,
                CreatedAt = consultation.CreatedAt,
                HasPrescription = false,
                PrescriptionId = null
            };
            _logger.LogInformation("Consultation DTO created successfully");

            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendConsultationCompletedNotificationAsync(consultation.Id, cancellationToken);
                }
                catch
                {
                }
            }, cancellationToken);

            return Result<ConsultationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consultation: {ErrorMessage}", ex.Message);
            return Result<ConsultationDto>.Failure(new[] { $"Failed to create consultation: {ex.Message}" });
        }
    }
}
