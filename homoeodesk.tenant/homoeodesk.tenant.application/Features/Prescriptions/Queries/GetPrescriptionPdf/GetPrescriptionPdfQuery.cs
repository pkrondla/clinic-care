using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescriptionPdf;

public record GetPrescriptionPdfQuery(
    int PrescriptionId,
    bool IncludeMedicineNames = true
) : IRequest<Result<byte[]>>;

