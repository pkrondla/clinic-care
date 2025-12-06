using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Services;
using ClinicCare.Application.Features.Invoices.Queries.GetInvoice;
using ClinicCare.Application.Features.Prescriptions.Queries.GetPrescription;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        IWhatsAppService whatsAppService,
        IEmailService emailService,
        ISmsService smsService,
        IMediator mediator,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _whatsAppService = whatsAppService;
        _emailService = emailService;
        _smsService = smsService;
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
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
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

            if (appointment == null || appointment.Patient?.User == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for reminder", appointmentId);
                return;
            }

            var patientPhone = appointment.Patient.User.Phone;
            var patientEmail = appointment.Patient.User.Email;
            var patientName = appointment.Patient.User.FullName;
            var doctorName = appointment.Doctor?.User?.FullName ?? "Doctor";
            var clinicName = appointment.Clinic?.Name ?? "Clinic";
            var appointmentDate = appointment.AppointmentDate.Value;
            var tokenNumber = appointment.TokenNumber;

            var message = $"Dear {patientName},\n\n" +
                         $"Reminder: Your appointment with Dr. {doctorName} at {clinicName} is scheduled for {appointmentDate:dd/MM/yyyy}.\n" +
                         $"Your token number is #{tokenNumber}.\n\n" +
                         $"Please arrive on time.\n\n" +
                         $"Thank you,\n{clinicName}";

            var emailSubject = $"Appointment Reminder - {clinicName}";
            var emailBody = $"<h2>Appointment Reminder</h2>" +
                           $"<p>Dear {patientName},</p>" +
                           $"<p>This is a reminder that your appointment with <strong>Dr. {doctorName}</strong> at <strong>{clinicName}</strong> is scheduled for <strong>{appointmentDate:dd/MM/yyyy}</strong>.</p>" +
                           $"<p>Your token number is <strong>#{tokenNumber}</strong>.</p>" +
                           $"<p>Please arrive on time.</p>" +
                           $"<p>Thank you,<br/>{clinicName}</p>";

            // Send via configured channels
            if (_configuration.GetValue<bool>("Features:EnableEmailNotifications") && !string.IsNullOrEmpty(patientEmail))
            {
                await _emailService.SendEmailAsync(patientEmail, emailSubject, emailBody, cancellationToken: cancellationToken);
            }

            if (_configuration.GetValue<bool>("Features:EnableWhatsAppIntegration") && !string.IsNullOrEmpty(patientPhone))
            {
                await _whatsAppService.SendTextMessageAsync(patientPhone, message, cancellationToken);
            }

            if (_configuration.GetValue<bool>("Features:EnableSMSNotifications") && !string.IsNullOrEmpty(patientPhone))
            {
                await _smsService.SendSmsAsync(patientPhone, message, cancellationToken);
            }
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
            var prescriptionResult = await _mediator.Send(new ClinicCare.Application.Features.Prescriptions.Queries.GetPrescription.GetPrescriptionQuery { Id = prescriptionId }, cancellationToken);
            if (!prescriptionResult.Succeeded || prescriptionResult.Data == null)
            {
                _logger.LogWarning("Prescription {PrescriptionId} not found for notification", prescriptionId);
                return;
            }

            var prescription = prescriptionResult.Data;
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == prescription.PatientId, cancellationToken);

            if (patient?.User == null)
            {
                return;
            }

            var patientPhone = patient.User.Phone;
            var patientEmail = patient.User.Email;
            var patientName = patient.User.FullName;

            var message = $"Dear {patientName},\n\n" +
                         $"Your prescription {prescription.PrescriptionNumber} is ready for collection.\n" +
                         $"Please visit the clinic to collect your medicines.\n\n" +
                         $"Thank you.";

            var emailSubject = "Prescription Ready for Collection";
            var emailBody = $"<h2>Prescription Ready</h2>" +
                           $"<p>Dear {patientName},</p>" +
                           $"<p>Your prescription <strong>{prescription.PrescriptionNumber}</strong> is ready for collection.</p>" +
                           $"<p>Please visit the clinic to collect your medicines.</p>" +
                           $"<p>Thank you.</p>";

            if (_configuration.GetValue<bool>("Features:EnableEmailNotifications") && !string.IsNullOrEmpty(patientEmail))
            {
                await _emailService.SendEmailAsync(patientEmail, emailSubject, emailBody, cancellationToken: cancellationToken);
            }

            if (_configuration.GetValue<bool>("Features:EnableWhatsAppIntegration") && !string.IsNullOrEmpty(patientPhone))
            {
                await _whatsAppService.SendTextMessageAsync(patientPhone, message, cancellationToken);
            }

            if (_configuration.GetValue<bool>("Features:EnableSMSNotifications") && !string.IsNullOrEmpty(patientPhone))
            {
                await _smsService.SendSmsAsync(patientPhone, message, cancellationToken);
            }
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
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == invoice.PatientId, cancellationToken);

            if (patient?.User == null)
            {
                return;
            }

            var patientPhone = patient.User.Phone;
            var patientEmail = patient.User.Email;
            var patientName = patient.User.FullName;

            var message = $"Dear {patientName},\n\n" +
                         $"Invoice {invoice.InvoiceNumber} has been generated.\n" +
                         $"Total Amount: ₹{invoice.TotalAmount:F2}\n" +
                         $"Please make payment to collect your medicines.\n\n" +
                         $"Thank you.";

            var emailSubject = $"Invoice {invoice.InvoiceNumber} - Payment Required";
            var emailBody = $"<h2>Invoice Generated</h2>" +
                           $"<p>Dear {patientName},</p>" +
                           $"<p>Invoice <strong>{invoice.InvoiceNumber}</strong> has been generated for your consultation.</p>" +
                           $"<p><strong>Total Amount: ₹{invoice.TotalAmount:F2}</strong></p>" +
                           $"<p>Please make payment to collect your medicines.</p>" +
                           $"<p>Thank you.</p>";

            if (_configuration.GetValue<bool>("Features:EnableEmailNotifications") && !string.IsNullOrEmpty(patientEmail))
            {
                await _emailService.SendEmailAsync(patientEmail, emailSubject, emailBody, cancellationToken: cancellationToken);
            }

            if (_configuration.GetValue<bool>("Features:EnableWhatsAppIntegration") && !string.IsNullOrEmpty(patientPhone))
            {
                await _whatsAppService.SendTextMessageAsync(patientPhone, message, cancellationToken);
            }

            if (_configuration.GetValue<bool>("Features:EnableSMSNotifications") && !string.IsNullOrEmpty(patientPhone))
            {
                await _smsService.SendSmsAsync(patientPhone, message, cancellationToken);
            }
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
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

            if (appointment == null || appointment.Patient?.User == null)
            {
                return;
            }

            var patientPhone = appointment.Patient.User.Phone;
            var patientName = appointment.Patient.User.FullName;
            var patientToken = appointment.TokenNumber;
            var clinicName = appointment.Clinic?.Name ?? "Clinic";

            if (currentToken >= patientToken - 2 && currentToken < patientToken)
            {
                var message = $"Dear {patientName},\n\n" +
                             $"Your token #{patientToken} will be called soon at {clinicName}.\n" +
                             $"Current token being served: #{currentToken}\n" +
                             $"Please be ready.\n\n" +
                             $"Thank you.";

                if (_configuration.GetValue<bool>("Features:EnableWhatsAppIntegration") && !string.IsNullOrEmpty(patientPhone))
                {
                    await _whatsAppService.SendTextMessageAsync(patientPhone, message, cancellationToken);
                }

                if (_configuration.GetValue<bool>("Features:EnableSMSNotifications") && !string.IsNullOrEmpty(patientPhone))
                {
                    await _smsService.SendSmsAsync(patientPhone, message, cancellationToken);
                }
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
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == invoice.PatientId, cancellationToken);

            if (patient?.User == null)
            {
                return;
            }

            var patientPhone = patient.User.Phone;
            var patientEmail = patient.User.Email;
            var patientName = patient.User.FullName;

            var message = $"Dear {patientName},\n\n" +
                         $"Your medicines have been dispatched via courier.\n" +
                         $"Courier Docket Number: {courierDocketNumber}\n" +
                         $"Invoice: {invoice.InvoiceNumber}\n" +
                         $"You can track your shipment using the docket number.\n\n" +
                         $"Thank you.";

            var emailSubject = $"Courier Dispatch - Docket {courierDocketNumber}";
            var emailBody = $"<h2>Medicines Dispatched</h2>" +
                           $"<p>Dear {patientName},</p>" +
                           $"<p>Your medicines have been dispatched via courier.</p>" +
                           $"<p><strong>Courier Docket Number: {courierDocketNumber}</strong></p>" +
                           $"<p>Invoice: {invoice.InvoiceNumber}</p>" +
                           $"<p>You can track your shipment using the docket number.</p>" +
                           $"<p>Thank you.</p>";

            if (_configuration.GetValue<bool>("Features:EnableEmailNotifications") && !string.IsNullOrEmpty(patientEmail))
            {
                await _emailService.SendEmailAsync(patientEmail, emailSubject, emailBody, cancellationToken: cancellationToken);
            }

            if (_configuration.GetValue<bool>("Features:EnableWhatsAppIntegration") && !string.IsNullOrEmpty(patientPhone))
            {
                await _whatsAppService.SendTextMessageAsync(patientPhone, message, cancellationToken);
            }

            if (_configuration.GetValue<bool>("Features:EnableSMSNotifications") && !string.IsNullOrEmpty(patientPhone))
            {
                await _smsService.SendSmsAsync(patientPhone, message, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send courier docket notification for invoice {InvoiceId}", invoiceId);
        }
    }
}

