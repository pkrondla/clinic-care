using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Common.Services;

public class InvoiceReadService : IInvoiceReadService
{
    private readonly IApplicationDbContext _context;

    public InvoiceReadService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InvoiceDto>> GetInvoiceDtoAsync(int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Branch)
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Consultation)
                .ThenInclude(c => c.Doctor)
                    .ThenInclude(d => d.User)
            .Include(i => i.Prescription)
                .ThenInclude(p => p.PrescriptionItems.OrderBy(pi => pi.Id))
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.IsActive, cancellationToken);

        if (invoice == null)
        {
            return Result<InvoiceDto>.Failure("Invoice not found");
        }

        var dto = new InvoiceDto
        {
            Id = invoice.Id,
            BranchId = invoice.BranchId,
            BranchName = invoice.Branch.Name,
            PatientId = invoice.PatientId,
            PatientName = invoice.Patient.User?.FullName ?? "Unknown",
            PatientCode = invoice.Patient.PatientCode,
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
            }).ToList(),
            PrescriptionItems = invoice.Prescription?.PrescriptionItems?.OrderBy(pi => pi.Id).Select(pi => new PrescriptionItemDto
            {
                Id = pi.Id,
                MedicineName = pi.MedicineName,
                DispensingForm = (int)pi.DispensingForm,
                Dosage = pi.Dosage,
                Frequency = pi.Frequency,
                Duration = pi.Duration,
                Timing = pi.Timing,
                Quantity = pi.Quantity,
                ContainerSize = pi.ContainerSize,
                Instructions = pi.Instructions,
                UnitPrice = pi.UnitPrice,
                TotalPrice = pi.TotalPrice
            }).ToList()
        };

        return Result<InvoiceDto>.Success(dto);
    }
}
