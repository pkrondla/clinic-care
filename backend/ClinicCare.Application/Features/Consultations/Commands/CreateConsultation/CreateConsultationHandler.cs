using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;

public class CreateConsultationHandler : IRequestHandler<CreateConsultationCommand, Result<ConsultationDto>>
{
    private readonly IConsultationRepository _repository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateConsultationHandler> _logger;
    private readonly INotificationService _notificationService;

    public CreateConsultationHandler(
        IConsultationRepository repository,
        IAppointmentRepository appointmentRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateConsultationHandler> logger,
        INotificationService notificationService)
    {
        _repository = repository;
        _appointmentRepository = appointmentRepository;
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

            // Verify appointment exists - use GetByIdWithDetailsAsync to load Patient and Doctor navigation properties
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(request.AppointmentId, cancellationToken);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found: {AppointmentId}", request.AppointmentId);
                return Result<ConsultationDto>.Failure(new[] { "Appointment not found." });
            }

            _logger.LogInformation("Appointment found: Id={Id}, PatientId={PatientId}, DoctorId={DoctorId}, OrganizationId={OrganizationId}", 
                appointment.Id, appointment.PatientId, appointment.DoctorId, appointment.OrganizationId);

            // Verify appointment belongs to the correct patient
            if (appointment.PatientId != request.PatientId)
            {
                _logger.LogWarning("Patient ID mismatch. Appointment PatientId: {AppointmentPatientId}, Request PatientId: {RequestPatientId}", 
                    appointment.PatientId, request.PatientId);
                return Result<ConsultationDto>.Failure(new[] { "Appointment does not belong to the specified patient." });
            }

            // Verify appointment belongs to the correct doctor (if doctor is specified)
            if (request.DoctorId > 0 && appointment.DoctorId != request.DoctorId)
            {
                _logger.LogWarning("Doctor ID mismatch. Appointment DoctorId: {AppointmentDoctorId}, Request DoctorId: {RequestDoctorId}", 
                    appointment.DoctorId, request.DoctorId);
                return Result<ConsultationDto>.Failure(new[] { "Appointment does not belong to the specified doctor." });
            }

            // Check if consultation already exists for this appointment
            var existing = await _repository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
            if (existing != null)
            {
                _logger.LogWarning("Consultation already exists for appointment: {AppointmentId}", request.AppointmentId);
                return Result<ConsultationDto>.Failure(new[] { "Consultation already exists for this appointment." });
            }

            // Create consultation
            var consultation = new Consultation
            {
                OrganizationId = appointment.OrganizationId, // Set OrganizationId from appointment
                AppointmentId = request.AppointmentId,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                ConsultationDate = DateTime.UtcNow,
                ConsultationType = (int)appointment.Type, // Set ConsultationType from AppointmentType (1 = InPerson, 2 = Teleconsultation)
                ChiefComplaint = request.ChiefComplaint,
                Symptoms = request.Symptoms ?? string.Empty,
                Examination = request.Examination ?? string.Empty,
                Diagnosis = request.Diagnosis ?? string.Empty,
                TreatmentPlan = request.TreatmentPlan ?? string.Empty,
                Notes = request.Notes ?? string.Empty,
                ConsultationFee = request.ConsultationFee
            };

            _logger.LogInformation("Adding consultation to repository...");
            var created = await _repository.AddAsync(consultation, cancellationToken);
            _logger.LogInformation("Consultation created successfully with Id: {ConsultationId}", created.Id);

            // Note: Appointment status update could be handled separately or via domain event
            // For now, we're keeping it simple

            // Manually create DTO since navigation properties aren't loaded after AddAsync
            // We use the appointment we already loaded which has Patient and Doctor navigation properties
            var dto = new ConsultationDto
            {
                Id = created.Id,
                AppointmentId = created.AppointmentId,
                PatientId = created.PatientId,
                PatientName = appointment.Patient?.User != null 
                    ? $"{appointment.Patient.User.FirstName ?? string.Empty} {appointment.Patient.User.LastName ?? string.Empty}".Trim()
                    : "Unknown",
                DoctorId = created.DoctorId,
                DoctorName = appointment.Doctor?.User != null 
                    ? $"{appointment.Doctor.User.FirstName ?? string.Empty} {appointment.Doctor.User.LastName ?? string.Empty}".Trim()
                    : "Unknown",
                ConsultationDate = created.ConsultationDate,
                ChiefComplaint = created.ChiefComplaint,
                Symptoms = created.Symptoms,
                Examination = created.Examination,
                Diagnosis = created.Diagnosis,
                TreatmentPlan = created.TreatmentPlan,
                Notes = created.Notes,
                ConsultationFee = created.ConsultationFee,
                CreatedAt = created.CreatedAt,
                HasPrescription = false, // New consultation doesn't have a prescription yet
                PrescriptionId = null
            };
            _logger.LogInformation("Consultation DTO created successfully");

            // Send consultation completed notification (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendConsultationCompletedNotificationAsync(created.Id, cancellationToken);
                }
                catch
                {
                    // Ignore notification errors - don't fail consultation creation
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

