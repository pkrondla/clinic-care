namespace ClinicCare.Application.Common.Services;

public interface INotificationService
{
    Task SendAppointmentReminderAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task SendPrescriptionReadyNotificationAsync(int prescriptionId, CancellationToken cancellationToken = default);
    Task SendInvoiceNotificationAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task SendTokenStatusUpdateAsync(int appointmentId, int currentToken, CancellationToken cancellationToken = default);
    Task SendCourierDocketNotificationAsync(int invoiceId, string courierDocketNumber, CancellationToken cancellationToken = default);
}

