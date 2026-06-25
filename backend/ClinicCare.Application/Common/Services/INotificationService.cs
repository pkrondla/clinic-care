namespace ClinicCare.Application.Common.Services;

public interface INotificationService
{
    Task SendAppointmentReminderAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task SendPrescriptionReadyNotificationAsync(int prescriptionId, CancellationToken cancellationToken = default);
    Task SendInvoiceNotificationAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task SendTokenStatusUpdateAsync(int appointmentId, int currentToken, CancellationToken cancellationToken = default);
    Task SendCourierDocketNotificationAsync(int invoiceId, string courierDocketNumber, CancellationToken cancellationToken = default);
    
    // New notification methods
    Task SendAppointmentCreatedNotificationAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task SendPrescriptionCreatedNotificationAsync(int prescriptionId, CancellationToken cancellationToken = default);
    Task SendPaymentReceivedNotificationAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task SendConsultationCompletedNotificationAsync(int consultationId, CancellationToken cancellationToken = default);
    Task SendAppointmentCancelledNotificationAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task SendPaymentReminderNotificationAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task SendCourierDeliveredNotificationAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task SendFollowUpReminderNotificationAsync(int patientId, int doctorId, DateTime followUpDate, CancellationToken cancellationToken = default);
}

