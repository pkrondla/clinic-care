using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Reports.Queries.GetCollectionReport;

public class GetCollectionReportHandler : IRequestHandler<GetCollectionReportQuery, Result<CollectionReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCollectionReportHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CollectionReportDto>> Handle(GetCollectionReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<CollectionReportDto>.Failure("User not associated with any organization");
            }

            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date.AddDays(1).AddTicks(-1); // End of day

            // Base query for invoices
            // CRITICAL: We avoid including Consultation.Appointment to prevent shadow property issues
            // The Appointment navigation is not needed for the collection report
            var invoicesQuery = _context.Invoices
                .Include(i => i.Clinic)
                .Include(i => i.Prescription)
                    .ThenInclude(p => p!.Consultation)
                        .ThenInclude(c => c!.Doctor)
                            .ThenInclude(d => d.User)
                // Explicitly exclude Appointment navigation to prevent EF Core from creating shadow properties
                .Where(i => i.OrganizationId == organizationId.Value
                    && i.InvoiceDate >= startDate
                    && i.InvoiceDate <= endDate
                    && i.IsActive);

            // Apply filters
            if (request.ClinicId.HasValue)
            {
                invoicesQuery = invoicesQuery.Where(i => i.ClinicId == request.ClinicId.Value);
            }

            if (request.DoctorId.HasValue)
            {
                invoicesQuery = invoicesQuery.Where(i => 
                    i.Prescription != null 
                    && i.Prescription.Consultation != null
                    && i.Prescription.Consultation.DoctorId == request.DoctorId.Value);
            }

            var invoices = await invoicesQuery.ToListAsync(cancellationToken);

            // Calculate totals
            var totalCollection = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount);
            var totalPending = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount);
            var totalInvoices = invoices.Count;
            var paidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid);
            var pendingInvoices = invoices.Count(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled);
            
            // If no invoices, return empty report
            if (totalInvoices == 0)
            {
                return Result<CollectionReportDto>.Success(new CollectionReportDto
                {
                    StartDate = startDate,
                    EndDate = request.EndDate.Date,
                    TotalCollection = 0,
                    TotalPending = 0,
                    TotalInvoices = 0,
                    PaidInvoices = 0,
                    PendingInvoices = 0,
                    Items = new List<CollectionReportItemDto>(),
                    PaymentMethodBreakdown = new List<PaymentMethodBreakdownDto>(),
                    DailyCollections = new List<DailyCollectionDto>()
                });
            }

            // Group items based on GroupBy parameter
            var items = new List<CollectionReportItemDto>();
            
            if (string.IsNullOrEmpty(request.GroupBy) || request.GroupBy == "day")
            {
                items = invoices
                    .GroupBy(i => i.InvoiceDate.Date)
                    .Select(g => new CollectionReportItemDto
                    {
                        GroupKey = g.Key.ToString("yyyy-MM-dd"),
                        TotalAmount = g.Sum(i => i.TotalAmount),
                        PaidAmount = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
                        BalanceAmount = g.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount),
                        InvoiceCount = g.Count()
                    })
                    .OrderBy(i => i.GroupKey)
                    .ToList();
            }
            else if (request.GroupBy == "week")
            {
                items = invoices
                    .GroupBy(i => GetWeekKey(i.InvoiceDate))
                    .Select(g => new CollectionReportItemDto
                    {
                        GroupKey = g.Key,
                        TotalAmount = g.Sum(i => i.TotalAmount),
                        PaidAmount = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
                        BalanceAmount = g.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount),
                        InvoiceCount = g.Count()
                    })
                    .OrderBy(i => i.GroupKey)
                    .ToList();
            }
            else if (request.GroupBy == "month")
            {
                items = invoices
                    .GroupBy(i => i.InvoiceDate.ToString("yyyy-MM"))
                    .Select(g => new CollectionReportItemDto
                    {
                        GroupKey = g.Key,
                        TotalAmount = g.Sum(i => i.TotalAmount),
                        PaidAmount = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
                        BalanceAmount = g.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount),
                        InvoiceCount = g.Count()
                    })
                    .OrderBy(i => i.GroupKey)
                    .ToList();
            }
            else if (request.GroupBy == "clinic")
            {
                items = invoices
                    .GroupBy(i => i.Clinic.Name)
                    .Select(g => new CollectionReportItemDto
                    {
                        GroupKey = g.Key,
                        TotalAmount = g.Sum(i => i.TotalAmount),
                        PaidAmount = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
                        BalanceAmount = g.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount),
                        InvoiceCount = g.Count()
                    })
                    .OrderBy(i => i.GroupKey)
                    .ToList();
            }
            else if (request.GroupBy == "doctor")
            {
                items = invoices
                    .Where(i => i.Prescription != null && i.Prescription.Consultation != null && i.Prescription.Consultation.Doctor != null)
                    .GroupBy(i => i.Prescription!.Consultation!.Doctor!.User.FirstName + " " + i.Prescription!.Consultation!.Doctor!.User.LastName)
                    .Select(g => new CollectionReportItemDto
                    {
                        GroupKey = g.Key,
                        TotalAmount = g.Sum(i => i.TotalAmount),
                        PaidAmount = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
                        BalanceAmount = g.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount),
                        InvoiceCount = g.Count()
                    })
                    .OrderBy(i => i.GroupKey)
                    .ToList();
            }

            // Payment method breakdown
            var paymentMethodBreakdown = invoices
                .Where(i => i.Status == InvoiceStatus.Paid && !string.IsNullOrEmpty(i.PaymentMethod))
                .GroupBy(i => i.PaymentMethod!)
                .Select(g => new PaymentMethodBreakdownDto
                {
                    PaymentMethod = g.Key,
                    Amount = g.Sum(i => i.PaidAmount),
                    Count = g.Count(),
                    Percentage = 0 // Will calculate below
                })
                .ToList();

            var totalPaid = paymentMethodBreakdown.Sum(p => p.Amount);
            foreach (var item in paymentMethodBreakdown)
            {
                item.Percentage = totalPaid > 0 ? (item.Amount / totalPaid) * 100 : 0;
            }

            // Daily collections
            var dailyCollections = invoices
                .Where(i => i.Status == InvoiceStatus.Paid)
                .GroupBy(i => i.InvoiceDate.Date)
                .Select(g => new DailyCollectionDto
                {
                    Date = g.Key,
                    Collection = g.Sum(i => i.PaidAmount),
                    InvoiceCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var report = new CollectionReportDto
            {
                StartDate = startDate,
                EndDate = request.EndDate.Date,
                TotalCollection = totalCollection,
                TotalPending = totalPending,
                TotalInvoices = totalInvoices,
                PaidInvoices = paidInvoices,
                PendingInvoices = pendingInvoices,
                Items = items,
                PaymentMethodBreakdown = paymentMethodBreakdown,
                DailyCollections = dailyCollections
            };

            return Result<CollectionReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<CollectionReportDto>.Failure($"Failed to generate collection report: {ex.Message}");
        }
    }

    private static string GetWeekKey(DateTime date)
    {
        var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);
        return $"{startOfWeek:yyyy-MM-dd} to {endOfWeek:yyyy-MM-dd}";
    }
}

