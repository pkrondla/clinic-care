using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HomoeoDesk.Tenant.Application.Common.Services;

/// <summary>
/// Service for processing notification templates with variable substitution
/// </summary>
public class NotificationTemplateService
{
    private readonly ILogger<NotificationTemplateService> _logger;
    private readonly Dictionary<NotificationType, string> _defaultTemplates;

    public NotificationTemplateService(ILogger<NotificationTemplateService> logger)
    {
        _logger = logger;
        _defaultTemplates = InitializeDefaultTemplates();
    }

    /// <summary>
    /// Process a template by replacing variables with actual values
    /// </summary>
    public string ProcessTemplate(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            _logger.LogWarning("Template is null or empty");
            return string.Empty;
        }

        var result = template;

        // Replace all variables in format {{VariableName}}
        foreach (var variable in variables)
        {
            var pattern = $@"\{{{{\s*{Regex.Escape(variable.Key)}\s*\}}}}";
            result = Regex.Replace(result, pattern, variable.Value ?? string.Empty, RegexOptions.IgnoreCase);
        }

        // Remove any remaining unreplaced variables (optional - can be configured)
        result = Regex.Replace(result, @"\{\{[\w]+\}\}", string.Empty);

        return result;
    }

    /// <summary>
    /// Get default template for a notification type
    /// </summary>
    public string GetDefaultTemplate(NotificationType notificationType)
    {
        return _defaultTemplates.TryGetValue(notificationType, out var template)
            ? template
            : string.Empty;
    }

    /// <summary>
    /// Validate template syntax
    /// </summary>
    public bool ValidateTemplate(string template, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(template))
        {
            errorMessage = "Template cannot be empty";
            return false;
        }

        // Check for unmatched braces
        var openBraces = Regex.Matches(template, @"\{\{").Count;
        var closeBraces = Regex.Matches(template, @"\}\}").Count;

        if (openBraces != closeBraces)
        {
            errorMessage = "Unmatched braces in template";
            return false;
        }

        // Check for valid variable syntax
        var invalidVariables = Regex.Matches(template, @"\{\{[^}]+\}\}")
            .Cast<Match>()
            .Where(m => !Regex.IsMatch(m.Value, @"\{\{\s*\w+\s*\}\}"))
            .ToList();

        if (invalidVariables.Any())
        {
            errorMessage = $"Invalid variable syntax: {string.Join(", ", invalidVariables.Select(v => v.Value))}";
            return false;
        }

        return true;
    }

    private Dictionary<NotificationType, string> InitializeDefaultTemplates()
    {
        return new Dictionary<NotificationType, string>
        {
            // Appointment Notifications
            [NotificationType.AppointmentCreated] = @"Dear {{PatientName}},

Your appointment has been confirmed!

📅 Date: {{AppointmentDate}}
🕐 Time: {{AppointmentTime}}
👨‍⚕️ Doctor: Dr. {{DoctorName}}
🏥 Clinic: {{BranchName}}
🎫 Token: #{{TokenNumber}}

Please arrive 10 minutes before your scheduled time.

Thank you,
{{BranchName}}",

            [NotificationType.AppointmentReminder] = @"Dear {{PatientName}},

Reminder: Your appointment with Dr. {{DoctorName}} is scheduled for {{AppointmentDate}} at {{AppointmentTime}}.

🎫 Your token number is #{{TokenNumber}}

📍 Location: {{ClinicAddress}}

Please arrive on time.

Thank you,
{{BranchName}}",

            [NotificationType.AppointmentCancelled] = @"Dear {{PatientName}},

Your appointment scheduled for {{AppointmentDate}} with Dr. {{DoctorName}} has been cancelled.

If you need to reschedule, please contact us.

Thank you,
{{BranchName}}",

            [NotificationType.TokenStatusUpdate] = @"Dear {{PatientName}},

Your token #{{PatientToken}} will be called soon at {{BranchName}}.

Current token being served: #{{CurrentToken}}

Please be ready.

Thank you,
{{BranchName}}",

            // Consultation Notifications
            [NotificationType.ConsultationCompleted] = @"Dear {{PatientName}},

Your consultation with Dr. {{DoctorName}} on {{ConsultationDate}} has been completed.

Your prescription and invoice details will be shared shortly.

Thank you,
{{BranchName}}",

            // Prescription Notifications
            [NotificationType.PrescriptionCreated] = @"Dear {{PatientName}},

Your prescription #{{PrescriptionNumber}} has been generated.

📋 Prescribed by: Dr. {{DoctorName}}
📅 Date: {{PrescriptionDate}}

Your medicines will be prepared shortly. You will receive another notification when ready for collection.

Thank you,
{{BranchName}}",

            [NotificationType.PrescriptionReadyForCollection] = @"Dear {{PatientName}},

Your prescription #{{PrescriptionNumber}} is ready for collection!

📍 Please visit {{BranchName}} to collect your medicines.

⏰ Clinic Hours: {{ClinicHours}}

Thank you,
{{BranchName}}",

            // Invoice Notifications
            [NotificationType.InvoiceCreated] = @"Dear {{PatientName}},

Invoice #{{InvoiceNumber}} has been generated for your consultation.

💰 Total Amount: ₹{{TotalAmount}}
📅 Date: {{InvoiceDate}}

Please make payment to collect your medicines.

Payment Methods:
- Cash
- UPI
- Card

Thank you,
{{BranchName}}",

            [NotificationType.PaymentReceived] = @"Dear {{PatientName}},

Payment of ₹{{AmountPaid}} has been received for Invoice #{{InvoiceNumber}}.

✅ Payment Status: Paid
📅 Payment Date: {{PaymentDate}}

Your medicines are ready for collection.

Thank you,
{{BranchName}}",

            [NotificationType.PaymentReminder] = @"Dear {{PatientName}},

Reminder: Payment pending for Invoice #{{InvoiceNumber}}.

💰 Amount Due: ₹{{AmountDue}}

Please make payment to collect your medicines.

Thank you,
{{BranchName}}",

            // Courier Notifications
            [NotificationType.CourierDispatched] = @"Dear {{PatientName}},

Your medicines have been dispatched via courier!

📦 Courier Docket: {{DocketNumber}}
📋 Invoice: #{{InvoiceNumber}}

You can track your shipment using the docket number.

Expected delivery: {{ExpectedDeliveryDate}}

Thank you,
{{BranchName}}",

            [NotificationType.CourierDelivered] = @"Dear {{PatientName}},

Your medicines have been delivered!

📦 Courier Docket: {{DocketNumber}}

Please check the package and confirm receipt.

Thank you,
{{BranchName}}",

            // Follow-up Notifications
            [NotificationType.FollowUpReminder] = @"Dear {{PatientName}},

This is a reminder for your follow-up appointment.

📅 Recommended Date: {{FollowUpDate}}
👨‍⚕️ Doctor: Dr. {{DoctorName}}

Please book your appointment at your earliest convenience.

Thank you,
{{BranchName}}"
        };
    }
}

