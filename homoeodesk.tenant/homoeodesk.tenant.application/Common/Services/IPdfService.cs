namespace HomoeoDesk.Tenant.Application.Common.Services;

public interface IPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePrescriptionPdfAsync(int prescriptionId, bool includeMedicineNames, CancellationToken cancellationToken = default);
}

