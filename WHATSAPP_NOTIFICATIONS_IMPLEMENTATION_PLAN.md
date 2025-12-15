# WhatsApp Notifications Implementation Plan

## Overview
This document outlines the implementation plan for integrating WhatsApp Business API notifications into the ClinicCare system. The system will allow administrators to configure WhatsApp Business settings per organization and send automated notifications for important events.

## Current State
- ✅ `NotificationService` exists with basic structure
- ✅ `IWhatsAppService` interface defined
- ✅ `WhatsAppService` placeholder implementation exists
- ✅ `Communication` entity for tracking sent messages
- ✅ Hangfire background jobs infrastructure with 'notifications' queue
- ✅ Basic notification methods for appointments, prescriptions, and invoices

## Architecture

### 1. Database Schema

#### New Entity: `WhatsAppBusinessSettings` (Tenant Database)
```sql
CREATE TABLE WhatsAppBusinessSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrganizationId INT NOT NULL,
    IsEnabled BIT NOT NULL DEFAULT 0,
    Provider NVARCHAR(50) NOT NULL, -- 'Meta', 'Twilio', '360dialog', etc.
    ApiKey NVARCHAR(500) NULL,
    ApiSecret NVARCHAR(500) NULL,
    PhoneNumberId NVARCHAR(100) NULL,
    BusinessAccountId NVARCHAR(100) NULL,
    AccessToken NVARCHAR(1000) NULL,
    WebhookUrl NVARCHAR(500) NULL,
    WebhookSecret NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);
```

#### New Entity: `NotificationPreferences` (Tenant Database)
```sql
CREATE TABLE NotificationPreferences (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrganizationId INT NOT NULL,
    NotificationType NVARCHAR(50) NOT NULL, -- 'AppointmentCreated', 'PrescriptionCreated', etc.
    EnableWhatsApp BIT NOT NULL DEFAULT 1,
    EnableEmail BIT NOT NULL DEFAULT 1,
    EnableSMS BIT NOT NULL DEFAULT 0,
    Template NVARCHAR(MAX) NULL, -- Custom message template
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    UNIQUE (OrganizationId, NotificationType)
);
```

### 2. Backend Components

#### A. Domain Layer
- **Entity**: `WhatsAppBusinessSettings` - Stores organization WhatsApp configuration
- **Entity**: `NotificationPreferences` - Stores per-organization notification preferences
- **Enum**: `NotificationType` - Enumeration of all notification types

#### B. Application Layer
- **Command**: `CreateOrUpdateWhatsAppSettingsCommand` - Admin configures WhatsApp settings
- **Query**: `GetWhatsAppSettingsQuery` - Retrieve organization WhatsApp settings
- **Command**: `UpdateNotificationPreferencesCommand` - Admin updates notification preferences
- **Query**: `GetNotificationPreferencesQuery` - Retrieve notification preferences
- **Service Interface**: `IWhatsAppProviderService` - Abstraction for different WhatsApp providers

#### C. Infrastructure Layer
- **Service**: `MetaWhatsAppService` - Implementation for Meta WhatsApp Business API
- **Service**: `TwilioWhatsAppService` - Implementation for Twilio WhatsApp API (optional)
- **Service**: `WhatsAppService` - Updated to use organization-specific settings
- **Service**: `NotificationTemplateService` - Handles message templating with variables

### 3. Frontend Components

#### A. Admin Settings Page
- **Route**: `/settings/whatsapp`
- **Component**: `WhatsAppSettingsPage.tsx`
  - Configure WhatsApp Business API credentials
  - Test connection
  - View webhook status
  - Enable/disable WhatsApp notifications

#### B. Notification Preferences Page
- **Route**: `/settings/notifications`
- **Component**: `NotificationPreferencesPage.tsx`
  - Toggle notifications per event type
  - Customize message templates
  - Preview messages

## Implementation Steps

### Phase 1: Database & Domain Layer
1. Create `WhatsAppBusinessSettings` entity
2. Create `NotificationPreferences` entity
3. Create `NotificationType` enum
4. Add EF Core configurations
5. Create database migration

### Phase 2: WhatsApp Provider Integration
1. Implement `MetaWhatsAppService` (Meta WhatsApp Business API)
2. Update `WhatsAppService` to load organization settings
3. Add provider factory pattern for multiple providers
4. Implement webhook handler for delivery status updates

### Phase 3: Notification Service Enhancement
1. Update `NotificationService` to check preferences
2. Add template engine for dynamic messages
3. Integrate with `Communication` entity for tracking
4. Add retry logic for failed messages

### Phase 4: Command/Query Handlers
1. Create WhatsApp settings CRUD handlers
2. Create notification preferences handlers
3. Add validation and error handling

### Phase 5: API Endpoints
1. `GET /api/settings/whatsapp` - Get WhatsApp settings
2. `POST /api/settings/whatsapp` - Create/Update settings
3. `POST /api/settings/whatsapp/test` - Test connection
4. `GET /api/settings/notifications` - Get preferences
5. `PUT /api/settings/notifications` - Update preferences
6. `POST /api/webhooks/whatsapp` - Webhook endpoint for status updates

### Phase 6: Frontend Implementation
1. Create WhatsApp settings page
2. Create notification preferences page
3. Add to Settings menu
4. Add test connection functionality

### Phase 7: Integration Points
1. Update `CreateAppointmentHandler` to send notification
2. Update `CreatePrescriptionHandler` to send notification
3. Update `CreateInvoiceHandler` to send notification
4. Update `CreateConsultationHandler` to send notification
5. Update `ProcessPaymentHandler` to send notification
6. Update existing Hangfire jobs to use new notification system

## List of WhatsApp Notifications

### 1. Appointment Notifications

#### 1.1 Appointment Created
**Trigger**: When a new appointment is booked
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Your appointment has been confirmed!

📅 Date: {{AppointmentDate}}
🕐 Time: {{AppointmentTime}}
👨‍⚕️ Doctor: Dr. {{DoctorName}}
🏥 Clinic: {{ClinicName}}
🎫 Token: #{{TokenNumber}}

Please arrive 10 minutes before your scheduled time.

Thank you,
{{ClinicName}}
```

#### 1.2 Appointment Reminder
**Trigger**: 24 hours before appointment (Hangfire job)
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Reminder: Your appointment with Dr. {{DoctorName}} is scheduled for {{AppointmentDate}} at {{AppointmentTime}}.

🎫 Your token number is #{{TokenNumber}}

📍 Location: {{ClinicAddress}}

Please arrive on time.

Thank you,
{{ClinicName}}
```

#### 1.3 Appointment Cancelled
**Trigger**: When appointment is cancelled
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Your appointment scheduled for {{AppointmentDate}} with Dr. {{DoctorName}} has been cancelled.

If you need to reschedule, please contact us.

Thank you,
{{ClinicName}}
```

#### 1.4 Token Status Update
**Trigger**: When current token is 2-3 tokens before patient's token
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Your token #{{PatientToken}} will be called soon at {{ClinicName}}.

Current token being served: #{{CurrentToken}}

Please be ready.

Thank you,
{{ClinicName}}
```

### 2. Consultation Notifications

#### 2.1 Consultation Completed
**Trigger**: When consultation is marked as completed
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Your consultation with Dr. {{DoctorName}} on {{ConsultationDate}} has been completed.

Your prescription and invoice details will be shared shortly.

Thank you,
{{ClinicName}}
```

### 3. Prescription Notifications

#### 3.1 Prescription Created
**Trigger**: When a new prescription is created
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Your prescription #{{PrescriptionNumber}} has been generated.

📋 Prescribed by: Dr. {{DoctorName}}
📅 Date: {{PrescriptionDate}}

Your medicines will be prepared shortly. You will receive another notification when ready for collection.

Thank you,
{{ClinicName}}
```

#### 3.2 Prescription Ready for Collection
**Trigger**: When prescription is marked as dispensed/ready
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Your prescription #{{PrescriptionNumber}} is ready for collection!

📍 Please visit {{ClinicName}} to collect your medicines.

⏰ Clinic Hours: {{ClinicHours}}

Thank you,
{{ClinicName}}
```

### 4. Invoice Notifications

#### 4.1 Invoice Created
**Trigger**: When an invoice is generated
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Invoice #{{InvoiceNumber}} has been generated for your consultation.

💰 Total Amount: ₹{{TotalAmount}}
📅 Date: {{InvoiceDate}}

Please make payment to collect your medicines.

Payment Methods:
- Cash
- UPI
- Card

Thank you,
{{ClinicName}}
```

#### 4.2 Payment Received
**Trigger**: When payment is processed
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Payment of ₹{{AmountPaid}} has been received for Invoice #{{InvoiceNumber}}.

✅ Payment Status: Paid
📅 Payment Date: {{PaymentDate}}

Your medicines are ready for collection.

Thank you,
{{ClinicName}}
```

#### 4.3 Payment Reminder
**Trigger**: 24 hours after invoice creation if unpaid (Hangfire job)
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Reminder: Payment pending for Invoice #{{InvoiceNumber}}.

💰 Amount Due: ₹{{AmountDue}}

Please make payment to collect your medicines.

Thank you,
{{ClinicName}}
```

### 5. Courier Notifications

#### 5.1 Courier Dispatched
**Trigger**: When courier docket is added to invoice
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Your medicines have been dispatched via courier!

📦 Courier Docket: {{DocketNumber}}
📋 Invoice: #{{InvoiceNumber}}

You can track your shipment using the docket number.

Expected delivery: {{ExpectedDeliveryDate}}

Thank you,
{{ClinicName}}
```

#### 5.2 Courier Delivered
**Trigger**: When courier status is updated to "Delivered" (via webhook or manual update)
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

Your medicines have been delivered!

📦 Courier Docket: {{DocketNumber}}

Please check the package and confirm receipt.

Thank you,
{{ClinicName}}
```

### 6. Follow-up Notifications

#### 6.1 Follow-up Reminder
**Trigger**: Based on prescription duration or doctor's recommendation
**Recipient**: Patient
**Template**:
```
Dear {{PatientName}},

This is a reminder for your follow-up appointment.

📅 Recommended Date: {{FollowUpDate}}
👨‍⚕️ Doctor: Dr. {{DoctorName}}

Please book your appointment at your earliest convenience.

Thank you,
{{ClinicName}}
```

## WhatsApp Business API Integration

### Provider Options

#### Option 1: Meta WhatsApp Business API (Recommended)
- **Pros**: Official API, direct integration, no third-party fees
- **Cons**: Requires Meta Business Account, verification process
- **Setup**: 
  1. Create Meta Business Account
  2. Create WhatsApp Business App
  3. Get Access Token
  4. Configure webhook for status updates

#### Option 2: Twilio WhatsApp API
- **Pros**: Easy integration, good documentation, reliable
- **Cons**: Per-message pricing
- **Setup**: 
  1. Create Twilio account
  2. Enable WhatsApp Sandbox (testing) or get approved number
  3. Get Account SID and Auth Token

#### Option 3: 360dialog
- **Pros**: Popular in India, good support
- **Cons**: Third-party service
- **Setup**: 
  1. Create account
  2. Get API key
  3. Configure webhook

### Implementation Details

#### Meta WhatsApp Business API Example
```csharp
public class MetaWhatsAppService : IWhatsAppProviderService
{
    private readonly HttpClient _httpClient;
    private readonly string _phoneNumberId;
    private readonly string _accessToken;
    
    public async Task<WhatsAppSendResult> SendTextMessageAsync(
        string to, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        var url = $"https://graph.facebook.com/v18.0/{_phoneNumberId}/messages";
        
        var payload = new
        {
            messaging_product = "whatsapp",
            to = to,
            type = "text",
            text = new { body = message }
        };
        
        var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        // Handle response and return result
    }
}
```

## Security Considerations

1. **Encrypt sensitive data**: Store API keys encrypted in database
2. **Webhook validation**: Verify webhook signatures from Meta
3. **Rate limiting**: Implement rate limiting to avoid API abuse
4. **Phone number validation**: Validate phone numbers before sending
5. **Opt-out mechanism**: Allow patients to opt-out of notifications
6. **Audit logging**: Log all notification attempts in `Communication` table

## Testing Strategy

1. **Unit Tests**: Test notification service, template engine
2. **Integration Tests**: Test WhatsApp API integration with mock responses
3. **E2E Tests**: Test full flow from event trigger to message delivery
4. **Webhook Tests**: Test webhook handling for delivery status

## Monitoring & Analytics

1. **Delivery Status**: Track message delivery status via webhooks
2. **Success Rate**: Monitor notification success/failure rates
3. **Cost Tracking**: Track API usage and costs per organization
4. **Error Logging**: Log all errors for debugging

## Future Enhancements

1. **Rich Media**: Support for images, PDFs (prescriptions, invoices)
2. **Interactive Messages**: Buttons for appointment confirmation, payment links
3. **Multi-language Support**: Templates in multiple languages
4. **Scheduled Messages**: Allow scheduling messages for specific times
5. **Message Templates**: Pre-approved templates for better deliverability
6. **Two-way Communication**: Handle patient replies and queries

## Configuration Example

```json
{
  "WhatsApp": {
    "Provider": "Meta",
    "PhoneNumberId": "123456789",
    "AccessToken": "encrypted_token",
    "WebhookVerifyToken": "your_verify_token",
    "ApiVersion": "v18.0"
  },
  "Notifications": {
    "DefaultEnabled": true,
    "RetryAttempts": 3,
    "RetryDelaySeconds": 60
  }
}
```

## Timeline Estimate

- **Phase 1-2**: 2-3 days (Database, Domain, Provider Integration)
- **Phase 3-4**: 2-3 days (Notification Service, Handlers)
- **Phase 5**: 1 day (API Endpoints)
- **Phase 6**: 2-3 days (Frontend)
- **Phase 7**: 2-3 days (Integration & Testing)
- **Total**: ~10-15 days

## Dependencies

- Meta WhatsApp Business API SDK (or HTTP client)
- Hangfire (already integrated)
- Entity Framework Core (already integrated)
- Frontend: Ant Design (already integrated)

