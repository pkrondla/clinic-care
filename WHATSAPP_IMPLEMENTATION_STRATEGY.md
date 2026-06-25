# WhatsApp Notifications Implementation Strategy

## Executive Summary

This document outlines the **best strategy** for implementing WhatsApp notifications in ClinicCare. The approach prioritizes **flexibility, scalability, and maintainability** while ensuring a smooth rollout.

## Recommended Strategy: Phased Approach with Provider Abstraction

### Core Principles

1. **Provider Abstraction**: Support multiple WhatsApp providers (Meta, Twilio, 360dialog) through a unified interface
2. **Organization-Level Configuration**: Each organization can configure their own WhatsApp settings
3. **Template-Based Messages**: Use templates with variable substitution for consistent messaging
4. **Background Processing**: Use Hangfire for async notification delivery
5. **Comprehensive Tracking**: Log all communications in the `Communication` entity
6. **Graceful Degradation**: System continues to work even if WhatsApp is unavailable

## Implementation Phases

### Phase 1: Foundation (Days 1-2) ⭐ START HERE

**Goal**: Set up the infrastructure without breaking existing functionality

#### 1.1 Database Schema
- Create `WhatsAppBusinessSettings` table (per organization)
- Create `NotificationPreferences` table (per organization, per notification type)
- Add indexes for performance
- Create migration script

#### 1.2 Domain Layer
- Create `WhatsAppBusinessSettings` entity
- Create `NotificationPreferences` entity
- Create `NotificationType` enum (15 notification types)
- Add EF Core configurations

#### 1.3 Provider Abstraction
- Create `IWhatsAppProviderService` interface (provider-agnostic)
- Create `WhatsAppProviderFactory` (selects provider based on settings)
- Update `IWhatsAppService` to use provider factory

**Why this order?**: Establishes the foundation without touching existing code.

---

### Phase 2: Meta WhatsApp Integration (Days 3-4) ⭐ RECOMMENDED PROVIDER

**Goal**: Implement Meta WhatsApp Business API (most common provider)

#### 2.1 Meta Provider Implementation
- Create `MetaWhatsAppProviderService` implementing `IWhatsAppProviderService`
- Implement text message sending
- Implement media message sending (for future PDFs)
- Add error handling and retry logic

#### 2.2 Configuration Management
- Create `GetWhatsAppSettingsQuery` handler
- Create `CreateOrUpdateWhatsAppSettingsCommand` handler
- Add validation (phone number format, token validation)
- Encrypt sensitive data (API keys, tokens)

#### 2.3 Testing Infrastructure
- Create test endpoint for sending test messages
- Add connection validation
- Mock provider for development/testing

**Why Meta first?**: 
- Official WhatsApp Business API
- Most widely used
- Good documentation
- No per-message fees (only setup costs)

---

### Phase 3: Template Engine (Days 5-6)

**Goal**: Dynamic message templates with variable substitution

#### 3.1 Template Service
- Create `NotificationTemplateService`
- Implement variable substitution (`{{PatientName}}`, `{{AppointmentDate}}`, etc.)
- Support for conditional content
- Template validation

#### 3.2 Default Templates
- Create default templates for all 15 notification types
- Store in database (can be customized per organization)
- Template preview functionality

#### 3.3 Template Variables
Define all available variables:
- Patient: `{{PatientName}}`, `{{PatientCode}}`, `{{PatientPhone}}`
- Appointment: `{{AppointmentDate}}`, `{{AppointmentTime}}`, `{{TokenNumber}}`
- Doctor: `{{DoctorName}}`
- Clinic: `{{ClinicName}}`, `{{ClinicAddress}}`, `{{ClinicHours}}`
- Prescription: `{{PrescriptionNumber}}`, `{{PrescriptionDate}}`
- Invoice: `{{InvoiceNumber}}`, `{{TotalAmount}}`, `{{AmountPaid}}`, `{{AmountDue}}`
- Courier: `{{DocketNumber}}`, `{{CourierCompany}}`, `{{TrackingUrl}}`

---

### Phase 4: Notification Preferences (Days 7-8)

**Goal**: Allow admins to control which notifications are sent

#### 4.1 Preferences Management
- Create `GetNotificationPreferencesQuery` handler
- Create `UpdateNotificationPreferencesCommand` handler
- Support enabling/disabling per notification type
- Support per-channel preferences (WhatsApp, Email, SMS)

#### 4.2 Integration with NotificationService
- Update `NotificationService` to check preferences before sending
- Respect organization-level and notification-type-level settings
- Log when notifications are skipped due to preferences

---

### Phase 5: Enhanced NotificationService (Days 9-10)

**Goal**: Integrate WhatsApp into existing notification flows

#### 5.1 Update Existing Methods
- `SendAppointmentReminderAsync` - Add WhatsApp support
- `SendPrescriptionReadyNotificationAsync` - Add WhatsApp support
- `SendInvoiceNotificationAsync` - Add WhatsApp support
- Add new methods for all 15 notification types

#### 5.2 Communication Tracking
- Create `Communication` record for every notification attempt
- Track status (Pending, Sent, Delivered, Failed)
- Store message content, recipient, timestamp
- Link to related entities (Appointment, Prescription, Invoice)

#### 5.3 Error Handling
- Retry logic (3 attempts with exponential backoff)
- Dead letter queue for failed messages
- Admin alerts for persistent failures

---

### Phase 6: Webhook Integration (Days 11-12)

**Goal**: Track message delivery status in real-time

#### 6.1 Webhook Endpoint
- Create `POST /api/webhooks/whatsapp` endpoint
- Verify webhook signature (security)
- Handle delivery status updates
- Update `Communication` records with status

#### 6.2 Status Updates
- Map provider statuses to `WhatsAppMessageStatus` enum
- Update `Communication.Status` based on webhook events
- Trigger follow-up actions (e.g., mark as delivered)

---

### Phase 7: API Endpoints (Day 13)

**Goal**: Expose settings and preferences via REST API

#### 7.1 Settings Endpoints
- `GET /api/settings/whatsapp` - Get WhatsApp settings
- `POST /api/settings/whatsapp` - Create/Update settings
- `POST /api/settings/whatsapp/test` - Test connection
- `DELETE /api/settings/whatsapp` - Delete settings

#### 7.2 Preferences Endpoints
- `GET /api/settings/notifications` - Get all preferences
- `PUT /api/settings/notifications/{type}` - Update specific preference
- `PUT /api/settings/notifications` - Bulk update

---

### Phase 8: Frontend Implementation (Days 14-16)

**Goal**: Admin UI for configuration and management

#### 8.1 WhatsApp Settings Page
- Form for API credentials
- Test connection button
- Status indicators (connected/disconnected)
- Webhook URL display
- Enable/disable toggle

#### 8.2 Notification Preferences Page
- Table/grid showing all notification types
- Toggle switches for each channel (WhatsApp, Email, SMS)
- Template editor with preview
- Save/cancel functionality

#### 8.3 Integration
- Add to Settings menu
- Role-based access (Admin only)
- Responsive design

---

### Phase 9: Integration Points (Days 17-18)

**Goal**: Connect notifications to business events

#### 9.1 Event Triggers
Update handlers to send notifications:
- `CreateAppointmentHandler` → Appointment Created notification
- `CreateConsultationHandler` → Consultation Completed notification
- `CreatePrescriptionHandler` → Prescription Created notification
- `CreateInvoiceHandler` → Invoice Created notification
- `PayInvoiceHandler` → Payment Received notification
- `UpdateCourierDocketHandler` → Courier Dispatched notification
- Hangfire jobs → Appointment Reminder, Payment Reminder

#### 9.2 Background Jobs
- Update existing Hangfire jobs to use new notification system
- Create new jobs for scheduled notifications
- Add retry policies

---

### Phase 10: Testing & Documentation (Days 19-20)

**Goal**: Ensure reliability and usability

#### 10.1 Testing
- Unit tests for template engine
- Integration tests for Meta provider
- E2E tests for notification flow
- Webhook testing

#### 10.2 Documentation
- Admin guide for setup
- API documentation
- Troubleshooting guide
- Template customization guide

---

## Recommended Provider: Meta WhatsApp Business API

### Why Meta?

1. **Official API**: Direct from WhatsApp/Meta
2. **No Per-Message Fees**: Only setup and verification costs
3. **Rich Features**: Supports text, media, interactive messages
4. **Reliable**: High deliverability rates
5. **Scalable**: Handles high volume
6. **Webhook Support**: Real-time delivery status

### Setup Requirements

1. **Meta Business Account** (free)
2. **WhatsApp Business App** (free)
3. **Phone Number** (can use existing or get new)
4. **Access Token** (from Meta)
5. **Webhook Configuration** (for status updates)

### Cost Estimate

- **Setup**: Free
- **Verification**: Free (for most countries)
- **Per Message**: Free (within conversation window)
- **Out of Window**: ~$0.005-0.01 per message (varies by country)

---

## Alternative Providers (Future)

### Twilio WhatsApp
- **Pros**: Easy setup, good documentation, reliable
- **Cons**: Per-message pricing (~$0.005-0.01)
- **Use Case**: If Meta setup is complex

### 360dialog
- **Pros**: Popular in India, good local support
- **Cons**: Third-party, pricing varies
- **Use Case**: If targeting Indian market specifically

---

## Security Best Practices

1. **Encrypt Sensitive Data**
   - Use .NET Data Protection API
   - Encrypt API keys, tokens, secrets
   - Store encrypted in database

2. **Webhook Security**
   - Verify webhook signatures
   - Use HTTPS only
   - Validate request source

3. **Rate Limiting**
   - Implement rate limits per organization
   - Prevent abuse
   - Monitor usage

4. **Phone Number Validation**
   - Validate format before sending
   - Check opt-out list
   - Respect privacy preferences

5. **Audit Logging**
   - Log all notification attempts
   - Track success/failure rates
   - Monitor for anomalies

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                    Business Events                       │
│  (Appointment, Prescription, Invoice, etc.)            │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              NotificationService                         │
│  - Check Preferences                                     │
│  - Load Template                                         │
│  - Substitute Variables                                  │
│  - Send via Channel (WhatsApp/Email/SMS)                │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│         WhatsAppProviderFactory                          │
│  - Load Organization Settings                            │
│  - Select Provider (Meta/Twilio/360dialog)               │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────┴───────────┐
         │                       │
         ▼                       ▼
┌──────────────────┐   ┌──────────────────┐
│ MetaWhatsApp     │   │ TwilioWhatsApp   │
│ ProviderService  │   │ ProviderService  │
└──────────────────┘   └──────────────────┘
         │                       │
         └───────────┬───────────┘
                     │
                     ▼
         ┌──────────────────────┐
         │  WhatsApp Business    │
         │       API             │
         └──────────────────────┘
                     │
                     ▼
         ┌──────────────────────┐
         │   Webhook Handler     │
         │  (Status Updates)     │
         └──────────────────────┘
                     │
                     ▼
         ┌──────────────────────┐
         │  Communication Entity │
         │  (Tracking & Logging) │
         └──────────────────────┘
```

---

## Implementation Checklist

### Phase 1: Foundation
- [ ] Create database tables (WhatsAppBusinessSettings, NotificationPreferences)
- [ ] Create domain entities
- [ ] Create EF Core configurations
- [ ] Create migration script
- [ ] Create IWhatsAppProviderService interface
- [ ] Create WhatsAppProviderFactory

### Phase 2: Meta Integration
- [ ] Implement MetaWhatsAppProviderService
- [ ] Add HttpClient configuration
- [ ] Implement SendTextMessageAsync
- [ ] Implement SendMediaMessageAsync
- [ ] Add error handling
- [ ] Create settings command/query handlers
- [ ] Add encryption for sensitive data

### Phase 3: Templates
- [ ] Create NotificationTemplateService
- [ ] Implement variable substitution
- [ ] Create default templates (15 types)
- [ ] Add template validation
- [ ] Add template preview

### Phase 4: Preferences
- [ ] Create preferences command/query handlers
- [ ] Update NotificationService to check preferences
- [ ] Add UI for preferences management

### Phase 5: Integration
- [ ] Update all notification methods
- [ ] Add Communication tracking
- [ ] Add retry logic
- [ ] Update Hangfire jobs

### Phase 6: Webhooks
- [ ] Create webhook endpoint
- [ ] Add signature verification
- [ ] Handle status updates
- [ ] Update Communication records

### Phase 7: API
- [ ] Create settings endpoints
- [ ] Create preferences endpoints
- [ ] Add test connection endpoint
- [ ] Add authentication/authorization

### Phase 8: Frontend
- [ ] Create WhatsApp settings page
- [ ] Create notification preferences page
- [ ] Add to Settings menu
- [ ] Add test connection UI

### Phase 9: Events
- [ ] Update CreateAppointmentHandler
- [ ] Update CreateConsultationHandler
- [ ] Update CreatePrescriptionHandler
- [ ] Update CreateInvoiceHandler
- [ ] Update PayInvoiceHandler
- [ ] Update UpdateCourierDocketHandler
- [ ] Update Hangfire jobs

### Phase 10: Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] E2E tests
- [ ] Documentation

---

## Quick Start (Minimal Viable Implementation)

If you want to get started quickly with a minimal implementation:

1. **Day 1**: Create database tables and entities
2. **Day 2**: Implement MetaWhatsAppProviderService (basic text messages only)
3. **Day 3**: Update NotificationService to use WhatsApp for 3-4 key events
4. **Day 4**: Create basic admin UI for settings
5. **Day 5**: Test and refine

This gives you a working WhatsApp notification system in 5 days, which you can then enhance with templates, preferences, webhooks, etc.

---

## Risk Mitigation

1. **Provider Outage**: System gracefully degrades, notifications logged but not sent
2. **API Changes**: Provider abstraction allows easy switching
3. **Cost Overruns**: Rate limiting and usage monitoring
4. **Privacy Concerns**: Opt-out mechanism, data encryption
5. **Delivery Failures**: Retry logic, dead letter queue, admin alerts

---

## Success Metrics

- **Delivery Rate**: >95% successful delivery
- **Response Time**: <2 seconds for notification trigger to API call
- **Uptime**: >99.9% availability
- **Cost**: <$0.01 per notification on average
- **User Satisfaction**: Reduced missed appointments, faster payment collection

---

## Next Steps

1. **Review this strategy** with the team
2. **Choose provider** (recommend Meta for start)
3. **Set up Meta Business Account** (if going with Meta)
4. **Start Phase 1** (Foundation)
5. **Iterate** through phases

---

## Questions to Consider

1. **Which provider?** → Start with Meta, add others later
2. **Which notifications first?** → Start with Appointment Reminder, Invoice Created, Payment Received
3. **Template customization?** → Start with defaults, add customization later
4. **Multi-language?** → Add after core functionality works
5. **Rich media?** → Add PDF support after text messages work

---

## Conclusion

This phased approach allows you to:
- ✅ Start small and iterate
- ✅ Test each phase before moving to next
- ✅ Maintain system stability
- ✅ Scale as needed
- ✅ Support multiple providers
- ✅ Customize per organization

**Recommended starting point**: Phase 1 (Foundation) → Phase 2 (Meta Integration) → Phase 3 (Templates) → Phase 5 (Integration)

This gives you a working system in ~10 days that can send WhatsApp notifications for key events.

