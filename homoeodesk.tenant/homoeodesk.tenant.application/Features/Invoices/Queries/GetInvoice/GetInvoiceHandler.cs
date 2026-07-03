using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoice;

public class GetInvoiceHandler : IRequestHandler<GetInvoiceQuery, Result<InvoiceDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IInvoiceReadService _invoiceReadService;

    public GetInvoiceHandler(
        ICurrentUserService currentUserService,
        IInvoiceReadService invoiceReadService)
    {
        _currentUserService = currentUserService;
        _invoiceReadService = invoiceReadService;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<InvoiceDto>.Failure("User not associated with any organization");
            }

            return await _invoiceReadService.GetInvoiceDtoAsync(request.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<InvoiceDto>.Failure($"Failed to retrieve invoice: {ex.Message}");
        }
    }
}

