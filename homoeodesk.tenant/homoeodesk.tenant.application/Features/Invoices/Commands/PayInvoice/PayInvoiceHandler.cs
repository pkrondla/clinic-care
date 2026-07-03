using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoice;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.PayInvoice;

public class PayInvoiceHandler : IRequestHandler<PayInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly INotificationService _notificationService;

    public PayInvoiceHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMediator mediator,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mediator = mediator;
        _notificationService = notificationService;
    }

    public async Task<Result<InvoiceDto>> Handle(PayInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<InvoiceDto>.Failure("User not associated with any organization");
            }

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId 
                                       && i.OrganizationId == organizationId.Value 
                                       && i.IsActive, cancellationToken);

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

            if (request.Amount <= 0)
            {
                return Result<InvoiceDto>.Failure("Payment amount must be greater than zero");
            }

            if (request.Amount > invoice.BalanceAmount)
            {
                return Result<InvoiceDto>.Failure("Payment amount cannot exceed the balance amount");
            }

            // Update payment details
            invoice.PaidAmount += request.Amount;
            invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;
            invoice.PaymentMethod = request.PaymentMethod;
            invoice.PaymentReference = request.PaymentReference ?? string.Empty;
            invoice.PaymentDate = DateTime.UtcNow;

            // Update status
            if (invoice.BalanceAmount <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
            else
            {
                // Partial payment - keep as Draft or Sent
                if (invoice.Status == InvoiceStatus.Draft)
                {
                    invoice.Status = InvoiceStatus.Sent;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Send payment received notification (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendPaymentReceivedNotificationAsync(request.InvoiceId, cancellationToken);
                }
                catch
                {
                    // Ignore notification errors - don't fail payment processing
                }
            }, cancellationToken);

            // Return updated invoice
            var getInvoiceQuery = new GetInvoiceQuery(request.InvoiceId);
            var result = await _mediator.Send(getInvoiceQuery, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<InvoiceDto>.Failure($"Failed to process payment: {ex.Message}");
        }
    }
}

