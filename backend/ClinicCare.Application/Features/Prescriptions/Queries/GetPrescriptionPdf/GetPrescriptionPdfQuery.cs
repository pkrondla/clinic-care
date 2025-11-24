using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Prescriptions.Queries.GetPrescriptionPdf;

public record GetPrescriptionPdfQuery(
    int PrescriptionId,
    bool IncludeMedicineNames = true
) : IRequest<Result<byte[]>>;

