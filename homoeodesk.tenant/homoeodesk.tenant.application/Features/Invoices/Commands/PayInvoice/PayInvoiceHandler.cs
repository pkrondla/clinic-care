using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.PayInvoice;

public class PayInvoiceHandler : IRequestHandler<PayInvoiceCommand, Result<InvoiceDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IInvoicePaymentService _invoicePaymentService;

    public PayInvoiceHandler(
        ICurrentUserService currentUserService,
        IInvoicePaymentService invoicePaymentService)
    {
        _currentUserService = currentUserService;
        _invoicePaymentService = invoicePaymentService;
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

            return await _invoicePaymentService.ApplyPaymentAsync(
                request.InvoiceId, request.Amount, request.PaymentMethod, request.PaymentReference, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<InvoiceDto>.Failure($"Failed to process payment: {ex.Message}");
        }
    }
}

