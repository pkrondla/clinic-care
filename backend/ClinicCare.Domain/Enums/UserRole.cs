namespace ClinicCare.Domain.Enums;

public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    Doctor = 3,
    Staff = 4,
    Patient = 5
}

public enum AppointmentType
{
    InPerson = 1,
    Teleconsultation = 2
}

public enum AppointmentStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum PrescriptionStatus
{
    Draft = 1,
    Issued = 2,
    Dispensed = 3
}

public enum TransactionType
{
    Purchase = 1,
    Sale = 2,
    Transfer = 3,
    Adjustment = 4
}

public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    Cancelled = 4
}

public enum CommunicationType
{
    WhatsApp = 1,
    Email = 2,
    SMS = 3
}

public enum CommunicationStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4
}

public enum CourierStatus
{
    NotDispatched = 0,
    Dispatched = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Returned = 5
}

public enum NotificationType
{
    // Appointment Notifications
    AppointmentCreated = 1,
    AppointmentReminder = 2,
    AppointmentCancelled = 3,
    TokenStatusUpdate = 4,
    
    // Consultation Notifications
    ConsultationCompleted = 5,
    
    // Prescription Notifications
    PrescriptionCreated = 6,
    PrescriptionReadyForCollection = 7,
    
    // Invoice Notifications
    InvoiceCreated = 8,
    PaymentReceived = 9,
    PaymentReminder = 10,
    
    // Courier Notifications
    CourierDispatched = 11,
    CourierDelivered = 12,
    
    // Follow-up Notifications
    FollowUpReminder = 13
}

public enum WhatsAppProvider
{
    Meta = 1,
    Twilio = 2,
    Dialog360 = 3
}