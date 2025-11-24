using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Domain.Entities;
using MediatR;

namespace ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionHandler : IRequestHandler<CreatePrescriptionCommand, Result<PrescriptionDto>>
{
    private readonly IPrescriptionRepository _repository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public CreatePrescriptionHandler(
        IPrescriptionRepository repository,
        IMapper mapper,
        INotificationService notificationService)
    {
        _repository = repository;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<Result<PrescriptionDto>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Generate prescription number
            var prescriptionNumber = await _repository.GeneratePrescriptionNumberAsync(cancellationToken);

            // Create prescription
            var prescription = new Prescription
            {
                PrescriptionNumber = prescriptionNumber,
                ConsultationId = request.ConsultationId,
                IssuedDate = DateTime.UtcNow,
                Status = Domain.Enums.PrescriptionStatus.Issued,
                PatientInstructions = request.Notes ?? string.Empty,
                InternalNotes = string.Empty,
                PrescriptionItems = request.Medicines.Select(m => new PrescriptionItem
                {
                    MedicineId = m.MedicineId,
                    MedicineName = m.MedicineName,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Duration = $"{m.Duration} days",
                    Quantity = m.Quantity,
                    UnitPrice = 0, // Will be calculated from medicine price
                    TotalPrice = 0, // Will be calculated
                    Instructions = m.Instructions ?? string.Empty
                }).ToList()
            };

            var created = await _repository.AddAsync(prescription, cancellationToken);
            var dto = _mapper.Map<PrescriptionDto>(created);

            // Send prescription ready notification (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendPrescriptionReadyNotificationAsync(dto.Id, cancellationToken);
                }
                catch
                {
                    // Ignore notification errors - don't fail prescription creation
                }
            }, cancellationToken);

            return Result<PrescriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PrescriptionDto>.Failure(new[] { $"Failed to create prescription: {ex.Message}" });
        }
    }
}

