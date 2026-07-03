using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InvoiceDto = HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription.InvoiceDto;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateInvoiceHandler> _logger;
    private readonly INotificationService _notificationService;

    public CreateInvoiceHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateInvoiceHandler> logger,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<InvoiceDto>.Failure("User not associated with any organization");
            }

            // Validate clinic
            var clinic = await _context.Branches
                .FirstOrDefaultAsync(c => c.Id == request.BranchId && c.OrganizationId == organizationId.Value && c.IsActive, cancellationToken);
            if (clinic == null)
            {
                return Result<InvoiceDto>.Failure("Branch not found");
            }

            // Validate patient
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId && p.OrganizationId == organizationId.Value && p.IsActive, cancellationToken);
            if (patient == null)
            {
                return Result<InvoiceDto>.Failure("Patient not found");
            }

            // Calculate totals from items
            decimal consultationAmount = 0;
            decimal medicineAmount = 0;
            decimal courierCharges = 0;

            var invoiceItems = new List<InvoiceItem>();
            foreach (var item in request.Items)
            {
                var totalPrice = item.UnitPrice * item.Quantity;

                switch (item.ItemType.ToLower())
                {
                    case "consultation":
                        consultationAmount += totalPrice;
                        break;
                    case "medicine":
                        medicineAmount += totalPrice;
                        break;
                    case "courier":
                        courierCharges += totalPrice;
                        break;
                }

                invoiceItems.Add(new InvoiceItem
                {
                    ItemType = item.ItemType,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = totalPrice,
                    OrganizationId = organizationId.Value
                });
            }

            // Use provided amounts or calculated amounts
            consultationAmount = request.ConsultationAmount > 0 ? request.ConsultationAmount : consultationAmount;
            medicineAmount = request.MedicineAmount > 0 ? request.MedicineAmount : medicineAmount;
            courierCharges = request.CourierCharges > 0 ? request.CourierCharges : courierCharges;

            decimal totalAmount = consultationAmount + medicineAmount + courierCharges;

            // Generate invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync(organizationId.Value, cancellationToken);

            // Create invoice
            var invoice = new Invoice
            {
                OrganizationId = organizationId.Value,
                BranchId = request.BranchId,
                PatientId = request.PatientId,
                ConsultationId = request.ConsultationId,
                PrescriptionId = request.PrescriptionId,
                InvoiceNumber = invoiceNumber,
                ConsultationAmount = consultationAmount,
                MedicineAmount = medicineAmount,
                CourierCharges = courierCharges,
                TotalAmount = totalAmount,
                PaidAmount = 0,
                BalanceAmount = totalAmount,
                Status = InvoiceStatus.Draft,
                PaymentMethod = string.Empty,
                PaymentReference = string.Empty,
                InvoiceDate = request.InvoiceDate ?? DateTime.UtcNow,
                PaymentDate = null,
                InvoiceItems = invoiceItems
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);

            // Reduce stock for medicine items
            foreach (var item in request.Items)
            {
                if (item.ItemType.Equals("Medicine", StringComparison.OrdinalIgnoreCase) && item.MedicineId.HasValue)
                {
                    // Find inventory for this medicine in the clinic
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.MedicineId == item.MedicineId.Value 
                                                && i.BranchId == request.BranchId 
                                                && i.OrganizationId == organizationId.Value 
                                                && i.IsActive, cancellationToken);

                    if (inventory != null)
                    {
                        // Check if sufficient stock is available
                        if (inventory.CurrentStock < item.Quantity)
                        {
                            _logger.LogWarning(
                                "Insufficient stock for medicine {MedicineId}. Available: {Available}, Requested: {Requested}",
                                item.MedicineId.Value, inventory.CurrentStock, item.Quantity);
                            // Continue but log warning - in production you might want to fail or handle differently
                        }

                        // Reduce stock
                        inventory.CurrentStock = Math.Max(0, inventory.CurrentStock - item.Quantity);
                        inventory.LastUpdated = DateTime.UtcNow;

                        // Create stock transaction record
                        var stockTransaction = new StockTransaction
                        {
                            OrganizationId = organizationId.Value,
                            BranchId = request.BranchId,
                            MedicineId = item.MedicineId.Value,
                            TransactionType = TransactionType.Sale,
                            Quantity = -item.Quantity, // Negative for sale
                            UnitPrice = item.UnitPrice,
                            Reference = invoice.InvoiceNumber,
                            Notes = $"Invoice sale - Invoice #{invoice.InvoiceNumber}",
                            TransactionDate = DateTime.UtcNow
                        };

                        _context.StockTransactions.Add(stockTransaction);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Inventory not found for medicine {MedicineId} in clinic {BranchId}",
                            item.MedicineId.Value, request.BranchId);
                    }
                }
            }

            // Save stock changes
            if (request.Items.Any(i => i.ItemType.Equals("Medicine", StringComparison.OrdinalIgnoreCase) && i.MedicineId.HasValue))
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Load invoice with details for response
            var invoiceWithDetails = await _context.Invoices
                .Include(i => i.Branch)
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Prescription)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

            if (invoiceWithDetails == null)
            {
                return Result<InvoiceDto>.Failure("Failed to retrieve created invoice");
            }

            // Map to DTO
            var dto = MapToDto(invoiceWithDetails);

            // Send invoice created notification (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendInvoiceNotificationAsync(dto.Id, cancellationToken);
                }
                catch
                {
                    // Ignore notification errors - don't fail invoice creation
                }
            }, cancellationToken);

            return Result<InvoiceDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice");
            return Result<InvoiceDto>.Failure($"Failed to create invoice: {ex.Message}");
        }
    }

    private async Task<string> GenerateInvoiceNumberAsync(int organizationId, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV{today:yyyyMMdd}";
        
        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix) && i.OrganizationId == organizationId)
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastInvoice == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = lastInvoice.InvoiceNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var sequence))
        {
            return $"{prefix}{(sequence + 1):D4}";
        }

        return $"{prefix}0001";
    }

    private InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            BranchId = invoice.BranchId,
            BranchName = invoice.Branch?.Name ?? "Unknown",
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

