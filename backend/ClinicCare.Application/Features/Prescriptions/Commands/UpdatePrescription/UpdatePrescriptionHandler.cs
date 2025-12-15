using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Application.Features.Prescriptions.Commands.UpdatePrescription;

public class UpdatePrescriptionHandler : IRequestHandler<UpdatePrescriptionCommand, Result<PrescriptionDto>>
{
    private readonly IPrescriptionRepository _repository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePrescriptionHandler> _logger;

    public UpdatePrescriptionHandler(
        IPrescriptionRepository repository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdatePrescriptionHandler> logger)
    {
        _repository = repository;
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PrescriptionDto>> Handle(UpdatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<PrescriptionDto>.Failure(new[] { "User not associated with any organization" });
            }

            // Get existing prescription
            var prescription = await _repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
            if (prescription == null)
            {
                return Result<PrescriptionDto>.Failure(new[] { "Prescription not found" });
            }

            // Verify organization match
            if (prescription.OrganizationId != organizationId.Value)
            {
                return Result<PrescriptionDto>.Failure(new[] { "Unauthorized access to prescription" });
            }

            // Check if invoice exists - if it does, we might want to prevent editing or show a warning
            // For now, we'll allow editing but the user should be aware
            var hasInvoice = await _context.Invoices
                .AnyAsync(i => i.PrescriptionId == prescription.Id && i.IsActive, cancellationToken);

            // Update notes
            if (request.Notes != null)
            {
                prescription.PatientInstructions = request.Notes;
            }

            // Remove existing prescription items
            _context.PrescriptionItems.RemoveRange(prescription.PrescriptionItems);

            // Add new prescription items
            prescription.PrescriptionItems = request.Medicines.Select(m =>
            {
                var quantity = m.Quantity ?? 1;
                decimal dispensedQuantity = 0;

                // Calculate dispensed quantity based on dispensing form
                switch ((DispensingForm)m.DispensingForm)
                {
                    case DispensingForm.Globules:
                        // For globules: quantity * containerSize * 4 drops per dram
                        var containerSize = m.ContainerSize ?? 1;
                        dispensedQuantity = quantity * containerSize * 4; // drops
                        break;
                    case DispensingForm.Tablets:
                        // For tablets: quantity is the count
                        dispensedQuantity = quantity;
                        break;
                    case DispensingForm.Packet:
                        // For packets: quantity * 5 drops per packet
                        dispensedQuantity = quantity * 5; // drops
                        break;
                    case DispensingForm.Liquid:
                        // For liquid: quantity in ml
                        dispensedQuantity = quantity; // ml
                        break;
                    case DispensingForm.Tonic:
                        // For tonic: quantity in ml
                        dispensedQuantity = quantity; // ml
                        break;
                    default:
                        dispensedQuantity = quantity;
                        break;
                }

                return new PrescriptionItem
                {
                    MedicineId = m.MedicineId,
                    MedicineName = m.MedicineName,
                    DispensingForm = (DispensingForm)m.DispensingForm,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Duration = m.Duration,
                    Timing = m.Timing,
                    ContainerSize = m.ContainerSize,
                    Quantity = m.Quantity,
                    DispensedQuantity = dispensedQuantity,
                    Instructions = m.Instructions ?? string.Empty,
                    OrganizationId = prescription.OrganizationId
                };
            }).ToList();

            prescription.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(prescription, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Reload prescription with details
            var updatedPrescription = await _repository.GetByIdWithDetailsAsync(prescription.Id, cancellationToken);
            if (updatedPrescription == null)
            {
                return Result<PrescriptionDto>.Failure(new[] { "Failed to retrieve updated prescription" });
            }

            var dto = _mapper.Map<PrescriptionDto>(updatedPrescription);

            // Check if invoice exists for this prescription
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.PrescriptionId == prescription.Id && i.IsActive, cancellationToken);

            dto.HasInvoice = invoice != null;
            dto.InvoiceId = invoice?.Id;

            _logger.LogInformation("Prescription {PrescriptionId} updated successfully", prescription.Id);

            return Result<PrescriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prescription {PrescriptionId}", request.Id);
            return Result<PrescriptionDto>.Failure(new[] { $"Failed to update prescription: {ex.Message}" });
        }
    }
}

