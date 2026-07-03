using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using HomoeoDesk.Tenant.Domain.Modules.Prescriptions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Events;

/// <summary>
/// Deducts dispensed quantities from inventory stock when a prescription is created.
/// Runs after the prescription is committed (via TenantDbContext's domain event dispatch),
/// so a deduction failure here never blocks prescription creation.
/// </summary>
public class DeductInventoryOnPrescriptionCreatedHandler : INotificationHandler<PrescriptionCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeductInventoryOnPrescriptionCreatedHandler> _logger;

    public DeductInventoryOnPrescriptionCreatedHandler(
        IApplicationDbContext context,
        ILogger<DeductInventoryOnPrescriptionCreatedHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(PrescriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        var prescriptionId = notification.PrescriptionId;

        try
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId, cancellationToken);
            if (prescription == null)
                return;

            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.Id == prescription.ConsultationId, cancellationToken);
            if (consultation == null)
                return;

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == consultation.AppointmentId, cancellationToken);
            if (appointment == null || appointment.BranchId <= 0)
                return;

            var branchId = appointment.BranchId;
            var organizationId = prescription.OrganizationId;

            foreach (var item in prescription.PrescriptionItems)
            {
                try
                {
                    // Skip stock deduction for custom medicines (no catalog MedicineId)
                    if (!item.MedicineId.HasValue || item.MedicineId.Value <= 0)
                    {
                        _logger.LogInformation(
                            "Skipping stock deduction for custom medicine: {MedicineName} (MedicineId is null or 0)",
                            item.MedicineName);
                        continue;
                    }

                    var inventory = await _context.Inventories
                        .Include(i => i.Medicine)
                        .FirstOrDefaultAsync(i => i.BranchId == branchId && i.MedicineId == item.MedicineId.Value, cancellationToken);
                    if (inventory == null)
                        continue;

                    // Dispensed quantity is in drops for Globules/Packet; convert to ml (1 drop = 0.05 ml).
                    // Liquid/Tonic/Tablets are already in stock units (ml or count).
                    var quantityToDeduct = item.DispensingForm switch
                    {
                        DispensingForm.Globules or DispensingForm.Packet => item.DispensedQuantity * 0.05m,
                        DispensingForm.Liquid or DispensingForm.Tonic or DispensingForm.Tablets => item.DispensedQuantity,
                        _ => 0m
                    };

                    var quantityToDeductInt = (int)Math.Round(quantityToDeduct);
                    if (inventory.CurrentStock < quantityToDeductInt)
                        quantityToDeductInt = Math.Min(quantityToDeductInt, inventory.CurrentStock);

                    if (quantityToDeductInt <= 0)
                        continue;

                    inventory.CurrentStock -= quantityToDeductInt;
                    inventory.LastUpdated = DateTime.UtcNow;
                    _context.Inventories.Update(inventory);

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        OrganizationId = organizationId,
                        BranchId = branchId,
                        MedicineId = item.MedicineId.Value,
                        TransactionType = TransactionType.Sale,
                        Quantity = quantityToDeductInt,
                        UnitPrice = inventory.SellingPrice,
                        Reference = prescription.PrescriptionNumber,
                        Notes = $"Prescription #{prescription.PrescriptionNumber} - {item.MedicineName}",
                        TransactionDate = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to deduct inventory for medicine {MedicineName} on prescription {PrescriptionId}",
                        item.MedicineName, prescriptionId);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deduct inventory for prescription {PrescriptionId}", prescriptionId);
        }
    }
}
