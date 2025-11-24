using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using ClinicCare.Application.Features.Invoices.Queries.GetInvoice;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Invoices.Commands.UpdateCourierDocket;

public class UpdateCourierDocketHandler : IRequestHandler<UpdateCourierDocketCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;

    public UpdateCourierDocketHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _mediator = mediator;
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
            var getInvoiceQuery = new GetInvoiceQuery(request.InvoiceId);
            var result = await _mediator.Send(getInvoiceQuery, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result<InvoiceDto>.Failure($"Failed to update courier docket: {ex.Message}");
        }
    }
}

