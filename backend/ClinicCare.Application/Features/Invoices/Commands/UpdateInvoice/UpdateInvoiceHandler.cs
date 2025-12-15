using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InvoiceDto = ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription.InvoiceDto;

namespace ClinicCare.Application.Features.Invoices.Commands.UpdateInvoice;

public class UpdateInvoiceHandler : IRequestHandler<UpdateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateInvoiceHandler> _logger;

    public UpdateInvoiceHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateInvoiceHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<InvoiceDto>> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<InvoiceDto>.Failure("User not associated with any organization");
            }

            // Get existing invoice
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == request.Id && i.OrganizationId == organizationId.Value && i.IsActive, cancellationToken);

            if (invoice == null)
            {
                return Result<InvoiceDto>.Failure("Invoice not found");
            }

            // Update clinic if provided
            if (request.ClinicId.HasValue)
            {
                var clinic = await _context.Clinics
                    .FirstOrDefaultAsync(c => c.Id == request.ClinicId.Value && c.OrganizationId == organizationId.Value && c.IsActive, cancellationToken);
                if (clinic == null)
                {
                    return Result<InvoiceDto>.Failure("Clinic not found");
                }
                invoice.ClinicId = request.ClinicId.Value;
            }

            // Update patient if provided
            if (request.PatientId.HasValue)
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.Id == request.PatientId.Value && p.OrganizationId == organizationId.Value && p.IsActive, cancellationToken);
                if (patient == null)
                {
                    return Result<InvoiceDto>.Failure("Patient not found");
                }
                invoice.PatientId = request.PatientId.Value;
            }

            // Update invoice items if provided
            if (request.Items != null && request.Items.Any())
            {
                // Remove existing items that are not in the new list
                var existingItemIds = request.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToList();
                var itemsToRemove = invoice.InvoiceItems.Where(item => !existingItemIds.Contains(item.Id)).ToList();
                _context.InvoiceItems.RemoveRange(itemsToRemove);

                // Update or add items
                foreach (var itemCommand in request.Items)
                {
                    if (itemCommand.Id.HasValue)
                    {
                        // Update existing item
                        var existingItem = invoice.InvoiceItems.FirstOrDefault(i => i.Id == itemCommand.Id.Value);
                        if (existingItem != null)
                        {
                            existingItem.ItemType = itemCommand.ItemType;
                            existingItem.Description = itemCommand.Description;
                            existingItem.Quantity = itemCommand.Quantity;
                            existingItem.UnitPrice = itemCommand.UnitPrice;
                            existingItem.TotalPrice = itemCommand.UnitPrice * itemCommand.Quantity;
                            existingItem.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // Add new item
                        invoice.InvoiceItems.Add(new InvoiceItem
                        {
                            ItemType = itemCommand.ItemType,
                            Description = itemCommand.Description,
                            Quantity = itemCommand.Quantity,
                            UnitPrice = itemCommand.UnitPrice,
                            TotalPrice = itemCommand.UnitPrice * itemCommand.Quantity,
                            OrganizationId = organizationId.Value
                        });
                    }
                }

                // Recalculate amounts from items
                decimal consultationAmount = 0;
                decimal medicineAmount = 0;
                decimal courierCharges = 0;

                foreach (var item in invoice.InvoiceItems)
                {
                    switch (item.ItemType.ToLower())
                    {
                        case "consultation":
                            consultationAmount += item.TotalPrice;
                            break;
                        case "medicine":
                            medicineAmount += item.TotalPrice;
                            break;
                        case "courier":
                            courierCharges += item.TotalPrice;
                            break;
                    }
                }

                invoice.ConsultationAmount = request.ConsultationAmount ?? consultationAmount;
                invoice.MedicineAmount = request.MedicineAmount ?? medicineAmount;
                invoice.CourierCharges = request.CourierCharges ?? courierCharges;
            }
            else
            {
                // Update amounts directly if provided
                if (request.ConsultationAmount.HasValue)
                    invoice.ConsultationAmount = request.ConsultationAmount.Value;
                if (request.MedicineAmount.HasValue)
                    invoice.MedicineAmount = request.MedicineAmount.Value;
                if (request.CourierCharges.HasValue)
                    invoice.CourierCharges = request.CourierCharges.Value;
            }

            // Recalculate total
            invoice.TotalAmount = invoice.ConsultationAmount + invoice.MedicineAmount + invoice.CourierCharges;
            invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;

            // Update status if provided
            if (request.Status.HasValue)
            {
                invoice.Status = (InvoiceStatus)request.Status.Value;
            }

            // Update invoice date if provided
            if (request.InvoiceDate.HasValue)
            {
                invoice.InvoiceDate = request.InvoiceDate.Value;
            }

            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Load invoice with details for response
            var invoiceWithDetails = await _context.Invoices
                .Include(i => i.Clinic)
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Prescription)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

            if (invoiceWithDetails == null)
            {
                return Result<InvoiceDto>.Failure("Failed to retrieve updated invoice");
            }

            // Map to DTO
            var dto = MapToDto(invoiceWithDetails);
            return Result<InvoiceDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId}", request.Id);
            return Result<InvoiceDto>.Failure($"Failed to update invoice: {ex.Message}");
        }
    }

    private InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            ClinicId = invoice.ClinicId,
            ClinicName = invoice.Clinic?.Name ?? "Unknown",
            PatientId = invoice.PatientId,
            PatientName = invoice.Patient?.User?.FullName ?? "Unknown",
            PatientCode = invoice.Patient?.PatientCode ?? string.Empty,
            ConsultationId = invoice.ConsultationId,
            PrescriptionId = invoice.PrescriptionId,
            PrescriptionNumber = invoice.Prescription?.PrescriptionNumber ?? string.Empty,
            InvoiceNumber = invoice.InvoiceNumber,
            ConsultationAmount = invoice.ConsultationAmount,
            MedicineAmount = invoice.MedicineAmount,
            CourierCharges = invoice.CourierCharges,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            BalanceAmount = invoice.BalanceAmount,
            Status = (int)invoice.Status,
            StatusText = invoice.Status switch
            {
                InvoiceStatus.Draft => "Draft",
                InvoiceStatus.Sent => "Sent",
                InvoiceStatus.Paid => "Paid",
                InvoiceStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            },
            PaymentMethod = invoice.PaymentMethod,
            PaymentReference = invoice.PaymentReference,
            InvoiceDate = invoice.InvoiceDate,
            PaymentDate = invoice.PaymentDate,
            CourierDocketNumber = invoice.CourierDocketNumber,
            CourierCompany = invoice.CourierCompany,
            CourierDispatchedDate = invoice.CourierDispatchedDate,
            CourierTrackingUrl = invoice.CourierTrackingUrl,
            CourierStatus = invoice.CourierStatus.HasValue ? (int)invoice.CourierStatus.Value : null,
            CourierStatusText = invoice.CourierStatus switch
            {
                CourierStatus.NotDispatched => "Not Dispatched",
                CourierStatus.Dispatched => "Dispatched",
                CourierStatus.InTransit => "In Transit",
                CourierStatus.OutForDelivery => "Out for Delivery",
                CourierStatus.Delivered => "Delivered",
                CourierStatus.Returned => "Returned",
                _ => null
            },
            Items = invoice.InvoiceItems.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                ItemType = item.ItemType,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }
}

