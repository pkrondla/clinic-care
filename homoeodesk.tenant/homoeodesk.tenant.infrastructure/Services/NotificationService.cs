using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoice;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescription;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationTemplateService _templateService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationService(
        IApplicationDbContext context,
        IWhatsAppService whatsAppService,
        IEmailService emailService,
        ISmsService smsService,
        IMediator mediator,
        IConfiguration configuration,
        ILogger<NotificationService> logger,
        NotificationTemplateService templateService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _whatsAppService = whatsAppService;
        _emailService = emailService;
        _smsService = smsService;
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _templateService = templateService;
        _currentUserService = currentUserService;
    }

    public async Task SendAppointmentReminderAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Branch)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

            if (appointment == null || appointment.Patient?.User == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for reminder", appointmentId);
                return;
            }

            var organizationId = appointment.OrganizationId;
            var patientId = appointment.PatientId;
            var patientName = appointment.Patient.User.FullName;
            var doctorName = appointment.Doctor?.User?.FullName ?? "Doctor";
            var BranchName = appointment.Branch?.Name ?? "Clinic";
            var clinicAddress = appointment.Branch?.Address ?? "";
            var appointmentDate = appointment.AppointmentDate.Value;
            var appointmentTime = appointment.AppointmentDate.Value.ToString("HH:mm");
            var tokenNumber = appointment.TokenNumber;

            // Prepare variables for template
            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DoctorName", doctorName },
                { "BranchName", BranchName },
                { "ClinicAddress", clinicAddress },
                { "AppointmentDate", appointmentDate.ToString("dd/MM/yyyy") },
                { "AppointmentTime", appointmentTime },
                { "TokenNumber", tokenNumber.ToString() }
            };

            // Send notification using template and preferences
            await SendNotificationAsync(
                NotificationType.AppointmentReminder,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment reminder for appointment {AppointmentId}", appointmentId);
        }
    }

    public async Task SendPrescriptionReadyNotificationAsync(int prescriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var prescriptionResult = await _mediator.Send(new HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescription.GetPrescriptionQuery { Id = prescriptionId }, cancellationToken);
            if (!prescriptionResult.Succeeded || prescriptionResult.Data == null)
            {
                _logger.LogWarning("Prescription {PrescriptionId} not found for notification", prescriptionId);
                return;
            }

            var prescription = prescriptionResult.Data;
            var prescriptionEntity = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Patient)
                        .ThenInclude(pat => pat.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Appointment)
                        .ThenInclude(a => a.Branch)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId, cancellationToken);

            if (prescriptionEntity == null || prescriptionEntity.Consultation?.Patient?.User == null)
            {
                return;
            }

            var organizationId = prescriptionEntity.OrganizationId;
            var patientId = prescriptionEntity.Consultation!.PatientId;
            var patientName = prescriptionEntity.Consultation.Patient.User.FullName;
            var doctorName = prescriptionEntity.Consultation.Doctor?.User?.FullName ?? "Doctor";
            var BranchName = prescriptionEntity.Consultation.Appointment?.Branch?.Name ?? "Clinic";
            var clinicHours = GetBranchHours(prescriptionEntity.Consultation.Appointment?.Branch);

            // Prepare variables for template
            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DoctorName", doctorName },
                { "BranchName", BranchName },
                { "ClinicHours", clinicHours },
                { "PrescriptionNumber", prescription.PrescriptionNumber },
                { "PrescriptionDate", prescription.PrescriptionDate.ToString("dd/MM/yyyy") }
            };

            // Send notification using template and preferences
            await SendNotificationAsync(
                NotificationType.PrescriptionReadyForCollection,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send prescription ready notification for prescription {PrescriptionId}", prescriptionId);
        }
    }

    public async Task SendInvoiceNotificationAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoiceResult = await _mediator.Send(new GetInvoiceQuery(invoiceId), cancellationToken);
            if (!invoiceResult.Succeeded || invoiceResult.Data == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for notification", invoiceId);
                return;
            }

            var invoice = invoiceResult.Data;
            var invoiceEntity = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

            if (invoiceEntity == null || invoiceEntity.Patient?.User == null)
            {
                return;
            }

            var organizationId = invoiceEntity.OrganizationId;
            var patientId = invoiceEntity.PatientId;
            var patientName = invoiceEntity.Patient.User.FullName;
            var BranchName = invoiceEntity.Branch?.Name ?? "Clinic";

            // Prepare variables for template
            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "BranchName", BranchName },
                { "InvoiceNumber", invoice.InvoiceNumber },
                { "TotalAmount", invoice.TotalAmount.ToString("F2") },
                { "InvoiceDate", invoice.InvoiceDate.ToString("dd/MM/yyyy") }
            };

            // Send notification using template and preferences
            await SendNotificationAsync(
                NotificationType.InvoiceCreated,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invoice notification for invoice {InvoiceId}", invoiceId);
        }
    }

    public async Task SendTokenStatusUpdateAsync(int appointmentId, int currentToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Branch)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

            if (appointment == null || appointment.Patient?.User == null)
            {
                return;
            }

            var organizationId = appointment.OrganizationId;
            var patientId = appointment.PatientId;
            var patientName = appointment.Patient.User.FullName;
            var patientToken = appointment.TokenNumber;
            var BranchName = appointment.Branch?.Name ?? "Clinic";

            if (currentToken >= patientToken - 2 && currentToken < patientToken)
            {
                // Prepare variables for template
                var variables = new Dictionary<string, string>
                {
                    { "PatientName", patientName },
                    { "BranchName", BranchName },
                    { "PatientToken", patientToken.ToString() },
                    { "CurrentToken", currentToken.ToString() }
                };

                // Send notification using template and preferences
                await SendNotificationAsync(
                    NotificationType.TokenStatusUpdate,
                    organizationId,
                    patientId,
                    variables,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send token status update for appointment {AppointmentId}", appointmentId);
        }
    }

    public async Task SendCourierDocketNotificationAsync(int invoiceId, string courierDocketNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoiceResult = await _mediator.Send(new GetInvoiceQuery(invoiceId), cancellationToken);
            if (!invoiceResult.Succeeded || invoiceResult.Data == null)
            {
                return;
            }

            var invoice = invoiceResult.Data;
            var invoiceEntity = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

            if (invoiceEntity == null || invoiceEntity.Patient?.User == null)
            {
                return;
            }

            var organizationId = invoiceEntity.OrganizationId;
            var patientId = invoiceEntity.PatientId;
            var patientName = invoiceEntity.Patient.User.FullName;
            var BranchName = invoiceEntity.Branch?.Name ?? "Clinic";
            var expectedDeliveryDate = DateTime.UtcNow.AddDays(3).ToString("dd/MM/yyyy"); // Default 3 days

            // Prepare variables for template
            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "BranchName", BranchName },
                { "DocketNumber", courierDocketNumber },
                { "InvoiceNumber", invoice.InvoiceNumber },
                { "ExpectedDeliveryDate", expectedDeliveryDate }
            };

            // Send notification using template and preferences
            await SendNotificationAsync(
                NotificationType.CourierDispatched,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send courier docket notification for invoice {InvoiceId}", invoiceId);
        }
    }

    /// <summary>
    /// Send notification using template and preferences
    /// </summary>
    private async Task SendNotificationAsync(
        NotificationType notificationType,
        int organizationId,
        int patientId,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check notification preferences
            var preference = await _context.NotificationPreferences
                .FirstOrDefaultAsync(
                    p => p.OrganizationId == organizationId 
                      && p.NotificationType == notificationType 
                      && p.IsActive,
                    cancellationToken);

            // If preference doesn't exist, use defaults (all enabled)
            var enableWhatsApp = preference?.EnableWhatsApp ?? true;
            var enableEmail = preference?.EnableEmail ?? true;
            var enableSMS = preference?.EnableSMS ?? false;

            // Check global settings - if globally disabled, override preference
            var whatsAppSettings = await _context.WhatsAppBusinessSettings
                .FirstOrDefaultAsync(
                    s => s.OrganizationId == organizationId 
                      && s.IsActive 
                      && s.IsEnabled,
                    cancellationToken);
            
            if (whatsAppSettings == null)
            {
                // WhatsApp not configured globally, disable it
                enableWhatsApp = false;
            }

            var emailSettings = await _context.EmailSettings
                .FirstOrDefaultAsync(
                    s => s.OrganizationId == organizationId 
                      && s.IsActive 
                      && s.IsEnabled,
                    cancellationToken);
            
            if (emailSettings == null)
            {
                // Email not configured globally, disable it
                enableEmail = false;
            }

            var smsSettings = await _context.SmsSettings
                .FirstOrDefaultAsync(
                    s => s.OrganizationId == organizationId 
                      && s.IsActive 
                      && s.IsEnabled,
                    cancellationToken);
            
            if (smsSettings == null)
            {
                // SMS not configured globally, disable it
                enableSMS = false;
            }

            // Get template (custom or default)
            var template = !string.IsNullOrEmpty(preference?.Template)
                ? preference.Template
                : _templateService.GetDefaultTemplate(notificationType);

            if (string.IsNullOrWhiteSpace(template))
            {
                _logger.LogWarning("No template found for notification type {NotificationType}", notificationType);
                return;
            }

            // Process template with variables
            var message = _templateService.ProcessTemplate(template, variables);

            // Get patient contact info
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

            if (patient?.User == null)
            {
                _logger.LogWarning("Patient {PatientId} not found for notification", patientId);
                return;
            }

            var patientPhone = patient.User.Phone;
            var patientEmail = patient.User.Email;

            // Send via enabled channels
            if (enableWhatsApp && !string.IsNullOrEmpty(patientPhone))
            {
                // Format phone number to E.164 if needed
                var formattedPhone = FormatPhoneNumber(patientPhone);
                if (!string.IsNullOrEmpty(formattedPhone))
                {
                    var result = await _whatsAppService.SendTextMessageAsync(formattedPhone, message, cancellationToken);
                    
                    // Log to Communication table
                    await LogCommunicationAsync(
                        patientId,
                        organizationId,
                        CommunicationType.WhatsApp,
                        notificationType.ToString(),
                        message,
                        formattedPhone,
                        result.Success ? CommunicationStatus.Sent : CommunicationStatus.Failed,
                        result.MessageId,
                        cancellationToken);
                }
            }

            if (enableEmail && !string.IsNullOrEmpty(patientEmail))
            {
                var emailSubject = GetEmailSubject(notificationType, variables);
                var emailBody = ConvertToHtml(message);
                await _emailService.SendEmailAsync(patientEmail, emailSubject, emailBody, cancellationToken: cancellationToken);
                
                await LogCommunicationAsync(
                    patientId,
                    organizationId,
                    CommunicationType.Email,
                    notificationType.ToString(),
                    message,
                    patientEmail,
                    CommunicationStatus.Sent,
                    null,
                    cancellationToken);
            }

            if (enableSMS && !string.IsNullOrEmpty(patientPhone))
            {
                var formattedPhone = FormatPhoneNumber(patientPhone);
                if (!string.IsNullOrEmpty(formattedPhone))
                {
                    await _smsService.SendSmsAsync(formattedPhone, message, cancellationToken);
                    
                    await LogCommunicationAsync(
                        patientId,
                        organizationId,
                        CommunicationType.SMS,
                        notificationType.ToString(),
                        message,
                        formattedPhone,
                        CommunicationStatus.Sent,
                        null,
                        cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification {NotificationType} for patient {PatientId}", 
                notificationType, patientId);
        }
    }

    /// <summary>
    /// Format phone number to E.164 format
    /// </summary>
    private string? FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        // Remove all non-digit characters except +
        var cleaned = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d+]", "");
        
        // If it doesn't start with +, log warning but still try
        // In production, you'd have country code mapping
        if (!cleaned.StartsWith("+"))
        {
            _logger.LogWarning("Phone number {Phone} is not in E.164 format. Please use +CountryCode format.", phone);
            // Try to add + if it's all digits (assume it's missing the +)
            if (System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^\d+$"))
            {
                // For now, return null. In production, map country code based on organization settings
                return null;
            }
        }

        return cleaned;
    }

    /// <summary>
    /// Get clinic hours as formatted string
    /// </summary>
    private string GetBranchHours(Domain.Entities.Branch? clinic)
    {
        if (clinic == null)
            return "Please contact clinic for hours";

        // Format clinic hours based on operating hours type
        // This is a simplified version - you can enhance based on your OperatingHoursType enum
        return "9:00 AM - 6:00 PM"; // Placeholder - enhance based on clinic.OperatingHoursType
    }

    /// <summary>
    /// Get email subject for notification type
    /// </summary>
    private string GetEmailSubject(NotificationType notificationType, Dictionary<string, string> variables)
    {
        var BranchName = variables.TryGetValue("BranchName", out var name) ? name : "Clinic";
        
        return notificationType switch
        {
            NotificationType.AppointmentCreated => $"Appointment Confirmed - {BranchName}",
            NotificationType.AppointmentReminder => $"Appointment Reminder - {BranchName}",
            NotificationType.AppointmentCancelled => $"Appointment Cancelled - {BranchName}",
            NotificationType.PrescriptionCreated => $"Prescription Generated - {BranchName}",
            NotificationType.PrescriptionReadyForCollection => $"Prescription Ready - {BranchName}",
            NotificationType.InvoiceCreated => $"Invoice Generated - {BranchName}",
            NotificationType.PaymentReceived => $"Payment Received - {BranchName}",
            NotificationType.CourierDispatched => $"Medicines Dispatched - {BranchName}",
            NotificationType.CourierDelivered => $"Medicines Delivered - {BranchName}",
            _ => $"Notification from {BranchName}"
        };
    }

    /// <summary>
    /// Convert plain text message to HTML
    /// </summary>
    private string ConvertToHtml(string plainText)
    {
        return plainText
            .Replace("\n", "<br/>");
    }

    /// <summary>
    /// Log communication to database
    /// </summary>
    private async Task LogCommunicationAsync(
        int patientId,
        int organizationId,
        CommunicationType type,
        string subject,
        string message,
        string recipientContact,
        CommunicationStatus status,
        string? reference,
        CancellationToken cancellationToken)
    {
        try
        {
            var communication = new Communication
            {
                PatientId = patientId,
                OrganizationId = organizationId,
                Type = type,
                Subject = subject,
                Message = message,
                RecipientContact = recipientContact,
                Status = status,
                Reference = reference ?? string.Empty,
                SentAt = status == CommunicationStatus.Sent ? DateTime.UtcNow : null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Communications.Add(communication);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log communication for patient {PatientId}", patientId);
        }
    }

    public async Task SendAppointmentCreatedNotificationAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Branch)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

            if (appointment == null || appointment.Patient?.User == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for notification", appointmentId);
                return;
            }

            var organizationId = appointment.OrganizationId;
            var patientId = appointment.PatientId;
            var patientName = appointment.Patient.User.FullName;
            var doctorName = appointment.Doctor?.User?.FullName ?? "Doctor";
            var BranchName = appointment.Branch?.Name ?? "Clinic";
            var appointmentDate = appointment.AppointmentDate.Value;
            var appointmentTime = appointment.AppointmentDate.Value.ToString("HH:mm");
            var tokenNumber = appointment.TokenNumber;

            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DoctorName", doctorName },
                { "BranchName", BranchName },
                { "AppointmentDate", appointmentDate.ToString("dd/MM/yyyy") },
                { "AppointmentTime", appointmentTime },
                { "TokenNumber", tokenNumber.ToString() }
            };

            await SendNotificationAsync(
                NotificationType.AppointmentCreated,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment created notification for appointment {AppointmentId}", appointmentId);
        }
    }

    public async Task SendPrescriptionCreatedNotificationAsync(int prescriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var prescriptionResult = await _mediator.Send(new HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescription.GetPrescriptionQuery { Id = prescriptionId }, cancellationToken);
            if (!prescriptionResult.Succeeded || prescriptionResult.Data == null)
            {
                _logger.LogWarning("Prescription {PrescriptionId} not found for notification", prescriptionId);
                return;
            }

            var prescription = prescriptionResult.Data;
            var prescriptionEntity = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Patient)
                        .ThenInclude(pat => pat.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Appointment)
                        .ThenInclude(a => a.Branch)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId, cancellationToken);

            if (prescriptionEntity == null || prescriptionEntity.Consultation?.Patient?.User == null)
            {
                return;
            }

            var organizationId = prescriptionEntity.OrganizationId;
            var patientId = prescriptionEntity.Consultation!.PatientId;
            var patientName = prescriptionEntity.Consultation.Patient.User.FullName;
            var doctorName = prescriptionEntity.Consultation.Doctor?.User?.FullName ?? "Doctor";
            var BranchName = prescriptionEntity.Consultation.Appointment?.Branch?.Name ?? "Clinic";

            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DoctorName", doctorName },
                { "BranchName", BranchName },
                { "PrescriptionNumber", prescription.PrescriptionNumber },
                { "PrescriptionDate", prescription.PrescriptionDate.ToString("dd/MM/yyyy") }
            };

            await SendNotificationAsync(
                NotificationType.PrescriptionCreated,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send prescription created notification for prescription {PrescriptionId}", prescriptionId);
        }
    }

    public async Task SendPaymentReceivedNotificationAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoiceResult = await _mediator.Send(new GetInvoiceQuery(invoiceId), cancellationToken);
            if (!invoiceResult.Succeeded || invoiceResult.Data == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for payment notification", invoiceId);
                return;
            }

            var invoice = invoiceResult.Data;
            var invoiceEntity = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

            if (invoiceEntity == null || invoiceEntity.Patient?.User == null)
            {
                return;
            }

            var organizationId = invoiceEntity.OrganizationId;
            var patientId = invoiceEntity.PatientId;
            var patientName = invoiceEntity.Patient.User.FullName;
            var BranchName = invoiceEntity.Branch?.Name ?? "Clinic";
            var paymentDate = invoiceEntity.PaymentDate?.ToString("dd/MM/yyyy HH:mm") ?? DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");

            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "BranchName", BranchName },
                { "InvoiceNumber", invoice.InvoiceNumber },
                { "AmountPaid", invoice.PaidAmount.ToString("F2") },
                { "PaymentDate", paymentDate }
            };

            await SendNotificationAsync(
                NotificationType.PaymentReceived,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment received notification for invoice {InvoiceId}", invoiceId);
        }
    }

    public async Task SendConsultationCompletedNotificationAsync(int consultationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var consultation = await _context.Consultations
                .Include(c => c.Patient)
                    .ThenInclude(p => p.User)
                .Include(c => c.Doctor)
                    .ThenInclude(d => d.User)
                .Include(c => c.Appointment)
                    .ThenInclude(a => a.Branch)
                .FirstOrDefaultAsync(c => c.Id == consultationId, cancellationToken);

            if (consultation == null || consultation.Patient?.User == null)
            {
                _logger.LogWarning("Consultation {ConsultationId} not found for notification", consultationId);
                return;
            }

            var organizationId = consultation.OrganizationId;
            var patientId = consultation.PatientId;
            var patientName = consultation.Patient.User.FullName;
            var doctorName = consultation.Doctor?.User?.FullName ?? "Doctor";
            var BranchName = consultation.Appointment?.Branch?.Name ?? "Clinic";
            var consultationDate = consultation.ConsultationDate.ToString("dd/MM/yyyy");

            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DoctorName", doctorName },
                { "BranchName", BranchName },
                { "ConsultationDate", consultationDate }
            };

            await SendNotificationAsync(
                NotificationType.ConsultationCompleted,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send consultation completed notification for consultation {ConsultationId}", consultationId);
        }
    }

    public async Task SendAppointmentCancelledNotificationAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Branch)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

            if (appointment == null || appointment.Patient?.User == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for cancellation notification", appointmentId);
                return;
            }

            var organizationId = appointment.OrganizationId;
            var patientId = appointment.PatientId;
            var patientName = appointment.Patient.User.FullName;
            var doctorName = appointment.Doctor?.User?.FullName ?? "Doctor";
            var BranchName = appointment.Branch?.Name ?? "Clinic";
            var appointmentDate = appointment.AppointmentDate.Value.ToString("dd/MM/yyyy");

            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DoctorName", doctorName },
                { "BranchName", BranchName },
                { "AppointmentDate", appointmentDate }
            };

            await SendNotificationAsync(
                NotificationType.AppointmentCancelled,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment cancelled notification for appointment {AppointmentId}", appointmentId);
        }
    }

    public async Task SendPaymentReminderNotificationAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoiceResult = await _mediator.Send(new GetInvoiceQuery(invoiceId), cancellationToken);
            if (!invoiceResult.Succeeded || invoiceResult.Data == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for payment reminder notification", invoiceId);
                return;
            }

            var invoice = invoiceResult.Data;
            var invoiceEntity = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

            if (invoiceEntity == null || invoiceEntity.Patient?.User == null)
            {
                return;
            }

            var organizationId = invoiceEntity.OrganizationId;
            var patientId = invoiceEntity.PatientId;
            var patientName = invoiceEntity.Patient.User.FullName;
            var BranchName = invoiceEntity.Branch?.Name ?? "Clinic";

            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "BranchName", BranchName },
                { "InvoiceNumber", invoice.InvoiceNumber },
                { "AmountDue", invoice.BalanceAmount.ToString("F2") }
            };

            await SendNotificationAsync(
                NotificationType.PaymentReminder,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment reminder notification for invoice {InvoiceId}", invoiceId);
        }
    }

    public async Task SendCourierDeliveredNotificationAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoiceResult = await _mediator.Send(new GetInvoiceQuery(invoiceId), cancellationToken);
            if (!invoiceResult.Succeeded || invoiceResult.Data == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for courier delivered notification", invoiceId);
                return;
            }

            var invoice = invoiceResult.Data;
            var invoiceEntity = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

            if (invoiceEntity == null || invoiceEntity.Patient?.User == null)
            {
                return;
            }

            var organizationId = invoiceEntity.OrganizationId;
            var patientId = invoiceEntity.PatientId;
            var patientName = invoiceEntity.Patient.User.FullName;
            var BranchName = invoiceEntity.Branch?.Name ?? "Clinic";
            var docketNumber = invoiceEntity.CourierDocketNumber ?? "N/A";

            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "BranchName", BranchName },
                { "DocketNumber", docketNumber }
            };

            await SendNotificationAsync(
                NotificationType.CourierDelivered,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send courier delivered notification for invoice {InvoiceId}", invoiceId);
        }
    }

    public async Task SendFollowUpReminderNotificationAsync(int patientId, int doctorId, DateTime followUpDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

            if (patient?.User == null)
            {
                _logger.LogWarning("Patient {PatientId} not found for follow-up reminder notification", patientId);
                return;
            }

            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

            if (doctor?.User == null)
            {
                _logger.LogWarning("Doctor {DoctorId} not found for follow-up reminder notification", doctorId);
                return;
            }

            var organizationId = patient.OrganizationId;
            var patientName = patient.User.FullName;
            var doctorName = doctor.User.FullName;
            
            // Get Branch Name from the most recent consultation or use default
            var recentConsultation = await _context.Consultations
                .Include(c => c.Appointment)
                    .ThenInclude(a => a.Branch)
                .Where(c => c.PatientId == patientId && c.DoctorId == doctorId)
                .OrderByDescending(c => c.ConsultationDate)
                .FirstOrDefaultAsync(cancellationToken);

            var BranchName = recentConsultation?.Appointment?.Branch?.Name ?? "Clinic";

            var variables = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DoctorName", doctorName },
                { "BranchName", BranchName },
                { "FollowUpDate", followUpDate.ToString("dd/MM/yyyy") }
            };

            await SendNotificationAsync(
                NotificationType.FollowUpReminder,
                organizationId,
                patientId,
                variables,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send follow-up reminder notification for patient {PatientId}", patientId);
        }
    }
}

