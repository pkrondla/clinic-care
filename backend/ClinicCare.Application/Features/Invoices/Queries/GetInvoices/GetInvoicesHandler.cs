using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Invoices.Queries.GetInvoices;

public class GetInvoicesHandler : IRequestHandler<GetInvoicesQuery, Result<List<InvoiceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInvoicesHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<InvoiceDto>>.Failure("User not associated with any organization");
            }

            var query = _context.Invoices
                .Include(i => i.Clinic)
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Prescription)
                .Include(i => i.InvoiceItems)
                .Where(i => i.OrganizationId == organizationId.Value && i.IsActive);

            if (request.ClinicId.HasValue)
            {
                query = query.Where(i => i.ClinicId == request.ClinicId.Value);
            }

            if (request.PatientId.HasValue)
            {
                query = query.Where(i => i.PatientId == request.PatientId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(i => (int)i.Status == request.Status.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(i => i.InvoiceDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(i => i.InvoiceDate <= request.EndDate.Value);
            }

            var invoices = await query
                .OrderByDescending(i => i.InvoiceDate)
                .ThenByDescending(i => i.Id)
                .ToListAsync(cancellationToken);

            var dtos = invoices.Select(invoice => new InvoiceDto
            {
                Id = invoice.Id,
                ClinicId = invoice.ClinicId,
                ClinicName = invoice.Clinic.Name,
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
                }).ToList()
            }).ToList();

            return Result<List<InvoiceDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<InvoiceDto>>.Failure($"Failed to retrieve invoices: {ex.Message}");
        }
    }
}

