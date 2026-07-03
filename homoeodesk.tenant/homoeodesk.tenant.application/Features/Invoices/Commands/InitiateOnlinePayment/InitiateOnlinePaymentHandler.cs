using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.InitiateOnlinePayment;

public class InitiateOnlinePaymentHandler : IRequestHandler<InitiateOnlinePaymentCommand, Result<OnlinePaymentInitiationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;

    public InitiateOnlinePaymentHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentGatewayFactory paymentGatewayFactory)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentGatewayFactory = paymentGatewayFactory;
    }

    public async Task<Result<OnlinePaymentInitiationDto>> Handle(InitiateOnlinePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<OnlinePaymentInitiationDto>.Failure("User not associated with any organization");
            }

            // Get invoice with patient details
            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId 
                    && i.OrganizationId == organizationId.Value 
                    && i.IsActive, cancellationToken);

            if (invoice == null)
            {
                return Result<OnlinePaymentInitiationDto>.Failure("Invoice not found");
            }

            if (invoice.Status == Domain.Enums.InvoiceStatus.Paid)
            {
                return Result<OnlinePaymentInitiationDto>.Failure("Invoice is already paid");
            }

            if (invoice.Status == Domain.Enums.InvoiceStatus.Cancelled)
            {
                return Result<OnlinePaymentInitiationDto>.Failure("Cannot process payment for a cancelled invoice");
            }

            // Get payment gateway
            var paymentGateway = _paymentGatewayFactory.GetPaymentGateway();

            // Prepare payment initiation request
            var paymentRequest = new PaymentInitiationRequest
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                Amount = invoice.BalanceAmount,
                Currency = "INR",
                CustomerName = invoice.Patient.User?.FullName ?? string.Empty,
                CustomerEmail = invoice.Patient.User?.Email ?? string.Empty,
                CustomerPhone = invoice.Patient.User?.Phone ?? string.Empty,
                ReturnUrl = request.ReturnUrl,
                CancelUrl = request.CancelUrl ?? request.ReturnUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "InvoiceId", invoice.Id.ToString() },
                    { "OrganizationId", organizationId.Value.ToString() },
                    { "BranchId", invoice.BranchId.ToString() }
                }
            };

            // Initiate payment
            var paymentResult = await paymentGateway.InitiatePaymentAsync(paymentRequest, cancellationToken);

            if (!paymentResult.Success)
            {
                return Result<OnlinePaymentInitiationDto>.Failure(
                    paymentResult.ErrorMessage ?? "Failed to initiate payment");
            }

            // Store transaction ID in invoice payment reference for webhook matching
            if (!string.IsNullOrWhiteSpace(paymentResult.TransactionId))
            {
                invoice.PaymentReference = paymentResult.TransactionId;
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Return payment initiation details
            var result = new OnlinePaymentInitiationDto
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                TransactionId = paymentResult.TransactionId ?? string.Empty,
                PaymentUrl = paymentResult.PaymentUrl ?? string.Empty,
                Amount = invoice.BalanceAmount,
                Currency = "INR",
                AdditionalData = paymentResult.AdditionalData
            };

            return Result<OnlinePaymentInitiationDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<OnlinePaymentInitiationDto>.Failure($"Failed to initiate online payment: {ex.Message}");
        }
    }
}

