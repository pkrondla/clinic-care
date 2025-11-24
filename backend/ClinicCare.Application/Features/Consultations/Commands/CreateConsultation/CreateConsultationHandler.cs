using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;

public class CreateConsultationHandler : IRequestHandler<CreateConsultationCommand, Result<ConsultationDto>>
{
    private readonly IConsultationRepository _repository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateConsultationHandler(
        IConsultationRepository repository,
        IAppointmentRepository appointmentRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _repository = repository;
        _appointmentRepository = appointmentRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<ConsultationDto>> Handle(CreateConsultationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify appointment exists
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
            if (appointment == null)
            {
                return Result<ConsultationDto>.Failure(new[] { "Appointment not found." });
            }

            // Check if consultation already exists for this appointment
            var existing = await _repository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
            if (existing != null)
            {
                return Result<ConsultationDto>.Failure(new[] { "Consultation already exists for this appointment." });
            }

            // Create consultation
            var consultation = new Consultation
            {
                AppointmentId = request.AppointmentId,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                ConsultationDate = DateTime.UtcNow,
                ChiefComplaint = request.ChiefComplaint,
                Symptoms = request.Symptoms ?? string.Empty,
                Examination = request.Examination ?? string.Empty,
                Diagnosis = request.Diagnosis ?? string.Empty,
                TreatmentPlan = request.TreatmentPlan ?? string.Empty,
                Notes = request.Notes ?? string.Empty,
                ConsultationFee = request.ConsultationFee
            };

            var created = await _repository.AddAsync(consultation, cancellationToken);

            // Note: Appointment status update could be handled separately or via domain event
            // For now, we're keeping it simple

            var dto = _mapper.Map<ConsultationDto>(created);
            return Result<ConsultationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ConsultationDto>.Failure(new[] { $"Failed to create consultation: {ex.Message}" });
        }
    }
}

