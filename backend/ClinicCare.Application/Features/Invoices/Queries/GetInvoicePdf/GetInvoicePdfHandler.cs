using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using MediatR;

namespace ClinicCare.Application.Features.Invoices.Queries.GetInvoicePdf;

public class GetInvoicePdfHandler : IRequestHandler<GetInvoicePdfQuery, Result<byte[]>>
{
    private readonly IPdfService _pdfService;

    public GetInvoicePdfHandler(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    public async Task<Result<byte[]>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(request.InvoiceId, cancellationToken);
            return Result<byte[]>.Success(pdfBytes);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"Failed to generate invoice PDF: {ex.Message}");
        }
    }
}

