using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescriptionPdf;

public class GetPrescriptionPdfHandler : IRequestHandler<GetPrescriptionPdfQuery, Result<byte[]>>
{
    private readonly IPdfService _pdfService;

    public GetPrescriptionPdfHandler(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    public async Task<Result<byte[]>> Handle(GetPrescriptionPdfQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await _pdfService.GeneratePrescriptionPdfAsync(
                request.PrescriptionId,
                request.IncludeMedicineNames,
                cancellationToken);
            return Result<byte[]>.Success(pdfBytes);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"Failed to generate prescription PDF: {ex.Message}");
        }
    }
}

