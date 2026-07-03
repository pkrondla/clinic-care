using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.UpdatePrescription;

public class UpdatePrescriptionHandler : IRequestHandler<UpdatePrescriptionCommand, Result<PrescriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePrescriptionHandler> _logger;

    public UpdatePrescriptionHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdatePrescriptionHandler> logger)
    {
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

            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(pat => pat!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Appointment)
                .Include(p => p.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (prescription == null)
            {
                return Result<PrescriptionDto>.Failure(new[] { "Prescription not found" });
            }

            if (prescription.OrganizationId != organizationId.Value)
            {
                return Result<PrescriptionDto>.Failure(new[] { "Unauthorized access to prescription" });
            }

            if (request.Notes != null)
            {
                prescription.PatientInstructions = request.Notes;
            }

            _context.PrescriptionItems.RemoveRange(prescription.PrescriptionItems);

            prescription.PrescriptionItems = request.Medicines.Select(m =>
            {
                var quantity = m.Quantity ?? 1;
                decimal dispensedQuantity = 0;

                switch ((DispensingForm)m.DispensingForm)
                {
                    case DispensingForm.Globules:
                        var containerSize = m.ContainerSize ?? 1;
                        dispensedQuantity = quantity * containerSize * 4;
                        break;
                    case DispensingForm.Tablets:
                        dispensedQuantity = quantity;
                        break;
                    case DispensingForm.Packet:
                        dispensedQuantity = quantity * 5;
                        break;
                    case DispensingForm.Liquid:
                    case DispensingForm.Tonic:
                        dispensedQuantity = quantity;
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

            _context.Prescriptions.Update(prescription);
            await _context.SaveChangesAsync(cancellationToken);

            var updatedPrescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(pat => pat!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Appointment)
                .Include(p => p.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.Id == prescription.Id, cancellationToken);

            if (updatedPrescription == null)
            {
                return Result<PrescriptionDto>.Failure(new[] { "Failed to retrieve updated prescription" });
            }

            var dto = _mapper.Map<PrescriptionDto>(updatedPrescription);

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
