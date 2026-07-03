using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Common.Services;

public class InvoicePaymentService : IInvoicePaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IInvoiceReadService _invoiceReadService;
    private readonly INotificationService _notificationService;

    public InvoicePaymentService(
        IApplicationDbContext context,
        IInvoiceReadService invoiceReadService,
        INotificationService notificationService)
    {
        _context = context;
        _invoiceReadService = invoiceReadService;
        _notificationService = notificationService;
    }

    public async Task<Result<InvoiceDto>> ApplyPaymentAsync(
        int invoiceId,
        decimal amount,
        string paymentMethod,
        string? paymentReference,
        CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.IsActive, cancellationToken);

        if (invoice == null)
        {
            return Result<InvoiceDto>.Failure("Invoice not found");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result<InvoiceDto>.Failure("Invoice is already paid");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result<InvoiceDto>.Failure("Cannot pay a cancelled invoice");
        }

        if (amount <= 0)
        {
            return Result<InvoiceDto>.Failure("Payment amount must be greater than zero");
        }

        if (amount > invoice.BalanceAmount)
        {
            return Result<InvoiceDto>.Failure("Payment amount cannot exceed the balance amount");
        }

        invoice.PaidAmount += amount;
        invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;
        invoice.PaymentMethod = paymentMethod;
        invoice.PaymentReference = paymentReference ?? string.Empty;
        invoice.PaymentDate = DateTime.UtcNow;

        if (invoice.BalanceAmount <= 0)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.Status == InvoiceStatus.Draft)
        {
            invoice.Status = InvoiceStatus.Sent;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.SendPaymentReceivedNotificationAsync(invoiceId, cancellationToken);
            }
            catch
            {
                // Ignore notification errors - don't fail payment processing
            }
        }, cancellationToken);

        return await _invoiceReadService.GetInvoiceDtoAsync(invoiceId, cancellationToken);
    }
}
