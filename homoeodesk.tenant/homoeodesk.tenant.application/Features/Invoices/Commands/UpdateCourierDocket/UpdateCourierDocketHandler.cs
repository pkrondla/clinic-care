using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.UpdateCourierDocket;

public class UpdateCourierDocketHandler : IRequestHandler<UpdateCourierDocketCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IInvoiceReadService _invoiceReadService;

    public UpdateCourierDocketHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IInvoiceReadService invoiceReadService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _invoiceReadService = invoiceReadService;
    }

    public async Task<Result<InvoiceDto>> Handle(UpdateCourierDocketCommand request, CancellationToken cancellationToken)
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

            // Update courier information
            invoice.CourierDocketNumber = request.CourierDocketNumber;
            invoice.CourierCompany = request.CourierCompany;
            invoice.CourierTrackingUrl = request.CourierTrackingUrl;
            invoice.CourierDispatchedDate = DateTime.UtcNow;
            invoice.CourierStatus = CourierStatus.Dispatched;

            await _context.SaveChangesAsync(cancellationToken);

            // Send courier docket notification (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendCourierDocketNotificationAsync(
                        request.InvoiceId,
                        request.CourierDocketNumber,
                        cancellationToken);
                }
                catch
                {
                    // Ignore notification errors
                }
            }, cancellationToken);

            // Return updated invoice
            return await _invoiceReadService.GetInvoiceDtoAsync(request.InvoiceId, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<InvoiceDto>.Failure($"Failed to update courier docket: {ex.Message}");
        }
    }
}

