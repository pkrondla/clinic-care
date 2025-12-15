# Event Workflows Explanation (Without Notifications)

This document explains how three key events work in the ClinicCare system **without considering notifications**. It focuses on the core business logic, data flow, and user interactions.

---

## 1. Prescription Ready for Collection

### Current Implementation Status
⚠️ **Partially Implemented**: The notification is sent when prescription is created, but there's **no explicit workflow to mark prescription as "ready"** in the system.

### How It Currently Works

#### A. Prescription Creation Flow

**1. Trigger Point:**
- **Location**: `CreatePrescriptionHandler.cs` (line 136)
- **When**: Doctor creates a prescription during/after consultation
- **Action**: Prescription is created with status `PrescriptionStatus.Issued` (default)

**2. Data Flow:**
```
User Action (Doctor)
    ↓
CreatePrescriptionCommand
    ↓
CreatePrescriptionHandler
    ↓
1. Create Prescription entity (Status = Issued)
2. Create PrescriptionItems
3. Deduct stock from inventory
4. Save to database
    ↓
NotificationService.SendPrescriptionReadyNotificationAsync() 
    (Fire-and-forget, doesn't block prescription creation)
```

**3. Database State:**
- **Table**: `Prescriptions`
- **Status Field**: `Status` (enum: `Draft = 1`, `Issued = 2`, `Dispensed = 3`)
- **Default Status**: `Issued` (2)
- **Current Behavior**: Prescription is created with `Status = Issued`, but notification is sent immediately

**4. Prescription Status Enum:**
```csharp
public enum PrescriptionStatus
{
    Draft = 1,      // Not yet finalized
    Issued = 2,     // Prescription created and issued to patient
    Dispensed = 3   // Medicines have been dispensed/ready for collection
}
```

### Missing Workflow

**What's Missing:**
- ❌ No command/handler to update prescription status from `Issued` to `Dispensed`
- ❌ No UI button/action to mark prescription as "ready for collection"
- ❌ No workflow for pharmacy staff to mark medicines as prepared

**What Should Happen:**
1. Prescription is created → Status = `Issued`
2. Pharmacy staff prepares medicines
3. Staff marks prescription as "Ready" → Status = `Dispensed`
4. **At this point**, notification should be sent (not when prescription is created)

### Recommended Implementation

**New Command Needed:**
```csharp
public record MarkPrescriptionReadyCommand(int PrescriptionId) 
    : IRequest<Result<PrescriptionDto>>;
```

**Handler Logic:**
```csharp
public async Task<Result<PrescriptionDto>> Handle(
    MarkPrescriptionReadyCommand request, 
    CancellationToken cancellationToken)
{
    var prescription = await _context.Prescriptions
        .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId, cancellationToken);
    
    if (prescription == null)
        return Result<PrescriptionDto>.Failure("Prescription not found");
    
    // Update status
    prescription.Status = PrescriptionStatus.Dispensed;
    prescription.UpdatedAt = DateTime.UtcNow;
    
    await _context.SaveChangesAsync(cancellationToken);
    
    // Send notification (fire-and-forget)
    _ = Task.Run(async () =>
    {
        await _notificationService.SendPrescriptionReadyNotificationAsync(
            prescription.Id, cancellationToken);
    });
    
    return Result<PrescriptionDto>.Success(mappedDto);
}
```

**Frontend UI Needed:**
- Add "Mark as Ready" button in `PrescriptionDetailPage.tsx`
- Show button only when `prescription.status === 2` (Issued)
- After clicking, update status to `Dispensed` (3)

---

## 2. Courier Dispatched

### Current Implementation Status
✅ **Fully Implemented**: Complete workflow exists for marking courier as dispatched.

### How It Works

#### A. Trigger Point

**1. User Action:**
- **Location**: Invoice Detail Page (`InvoiceDetailPage.tsx`)
- **When**: Staff receives courier docket information for an invoice with courier charges
- **UI**: "Update Courier Docket" button (line 495-502)
- **Condition**: Button appears only if `invoice.courierCharges > 0 && !invoice.courierDocketNumber`

**2. User Flow:**
```
Invoice Detail Page
    ↓
User clicks "Update Courier Docket" button
    ↓
Modal opens with form fields:
    - Courier Docket Number (required)
    - Courier Company (required)
    - Tracking URL (optional)
    ↓
User fills form and clicks OK
    ↓
UpdateCourierDocketCommand
```

#### B. Backend Processing

**1. Command:**
```csharp
// UpdateCourierDocketCommand.cs
public record UpdateCourierDocketCommand(
    int InvoiceId,
    [Required] string CourierDocketNumber,
    [Required] string CourierCompany,
    string? CourierTrackingUrl = null
) : IRequest<Result<InvoiceDto>>;
```

**2. Handler Logic (`UpdateCourierDocketHandler.cs`):**
```csharp
// Step 1: Find invoice
var invoice = await _context.Invoices
    .FirstOrDefaultAsync(i => i.Id == request.InvoiceId 
                           && i.OrganizationId == organizationId.Value 
                           && i.IsActive, cancellationToken);

// Step 2: Update courier fields
invoice.CourierDocketNumber = request.CourierDocketNumber;
invoice.CourierCompany = request.CourierCompany;
invoice.CourierTrackingUrl = request.CourierTrackingUrl;
invoice.CourierDispatchedDate = DateTime.UtcNow;  // Auto-set to current time
invoice.CourierStatus = CourierStatus.Dispatched;  // Set to Dispatched (1)

// Step 3: Save changes
await _context.SaveChangesAsync(cancellationToken);

// Step 4: Send notification (fire-and-forget)
_ = Task.Run(async () =>
{
    await _notificationService.SendCourierDocketNotificationAsync(
        request.InvoiceId,
        request.CourierDocketNumber,
        cancellationToken);
});
```

**3. Database Updates:**
- **Table**: `Invoices`
- **Fields Updated**:
  - `CourierDocketNumber` → Set to provided value
  - `CourierCompany` → Set to provided value
  - `CourierTrackingUrl` → Set to provided value (nullable)
  - `CourierDispatchedDate` → Set to `DateTime.UtcNow`
  - `CourierStatus` → Set to `CourierStatus.Dispatched` (1)

**4. Courier Status Enum:**
```csharp
public enum CourierStatus
{
    NotDispatched = 0,
    Dispatched = 1,        // ← Set when docket is added
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Returned = 5
}
```

#### C. Frontend Implementation

**1. UI Components:**
- **Button**: "Update Courier Docket" (line 495-502 in `InvoiceDetailPage.tsx`)
- **Modal**: Courier docket form (line 600-633)
- **Form Fields**:
  - Courier Docket Number (required)
  - Courier Company (required)
  - Tracking URL (optional)

**2. Display:**
After update, courier information is displayed in invoice details:
- Courier Docket Number (bold)
- Courier Company
- Courier Status (tag with color coding)
- Tracking URL (clickable link)

**3. Status Color Coding:**
```typescript
<Tag color={
  invoice.courierStatus === 4 ? 'green' :    // Delivered
  invoice.courierStatus === 5 ? 'red' :     // Returned
  invoice.courierStatus === 3 ? 'orange' :  // Out for Delivery
  'blue'                                      // Dispatched/In Transit
}>
  {invoice.courierStatusText}
</Tag>
```

### Complete Workflow Diagram

```
┌─────────────────────────────────────┐
│  Invoice Detail Page                │
│  (Invoice with courier charges)     │
└──────────────┬──────────────────────┘
               │
               │ User clicks
               │ "Update Courier Docket"
               ▼
┌─────────────────────────────────────┐
│  Courier Docket Modal               │
│  - Docket Number (required)         │
│  - Company (required)                │
│  - Tracking URL (optional)           │
└──────────────┬──────────────────────┘
               │
               │ User submits form
               ▼
┌─────────────────────────────────────┐
│  UpdateCourierDocketCommand         │
│  → UpdateCourierDocketHandler       │
└──────────────┬──────────────────────┘
               │
               ├─→ Update Invoice:
               │   - CourierDocketNumber
               │   - CourierCompany
               │   - CourierTrackingUrl
               │   - CourierDispatchedDate = Now
               │   - CourierStatus = Dispatched
               │
               └─→ Send Notification
                   (fire-and-forget)
```

---

## 3. Courier Delivered

### Current Implementation Status
⚠️ **Not Implemented**: There's **no workflow to mark courier as delivered** in the current system.

### Current State

**1. Database Support:**
- ✅ `CourierStatus` enum includes `Delivered = 4`
- ✅ `Invoices` table has `CourierStatus` column
- ✅ Frontend displays courier status with color coding

**2. Missing Implementation:**
- ❌ No command to update courier status to `Delivered`
- ❌ No UI button/action to mark as delivered
- ❌ No workflow for staff to confirm delivery

### How It Should Work

#### A. Recommended Implementation

**1. New Command:**
```csharp
public record UpdateCourierStatusCommand(
    int InvoiceId,
    CourierStatus Status  // Can be InTransit, OutForDelivery, Delivered, Returned
) : IRequest<Result<InvoiceDto>>;
```

**2. Handler Logic:**
```csharp
public async Task<Result<InvoiceDto>> Handle(
    UpdateCourierStatusCommand request, 
    CancellationToken cancellationToken)
{
    var invoice = await _context.Invoices
        .FirstOrDefaultAsync(i => i.Id == request.InvoiceId 
                               && i.OrganizationId == organizationId.Value 
                               && i.IsActive, cancellationToken);
    
    if (invoice == null)
        return Result<InvoiceDto>.Failure("Invoice not found");
    
    if (invoice.CourierStatus == null)
        return Result<InvoiceDto>.Failure("Courier not yet dispatched");
    
    // Update status
    invoice.CourierStatus = request.Status;
    invoice.UpdatedAt = DateTime.UtcNow;
    
    // If delivered, set delivery date
    if (request.Status == CourierStatus.Delivered)
    {
        // Could add CourierDeliveredDate field if needed
    }
    
    await _context.SaveChangesAsync(cancellationToken);
    
    // Send notification if delivered (fire-and-forget)
    if (request.Status == CourierStatus.Delivered)
    {
        _ = Task.Run(async () =>
        {
            await _notificationService.SendCourierDeliveredNotificationAsync(
                invoice.Id, cancellationToken);
        });
    }
    
    return Result<InvoiceDto>.Success(mappedDto);
}
```

**3. Frontend UI:**
- Add status dropdown/buttons in `InvoiceDetailPage.tsx`
- Show only when `invoice.courierDocketNumber` exists
- Options:
  - "Mark as In Transit"
  - "Mark as Out for Delivery"
  - "Mark as Delivered" ← **This triggers the notification**
  - "Mark as Returned"

**4. UI Location:**
In `InvoiceDetailPage.tsx`, add status update section:
```typescript
{invoice.courierDocketNumber && invoice.courierStatus !== 4 && (
  <Card>
    <Title level={5}>Update Courier Status</Title>
    <Space>
      <Button onClick={() => updateStatus(2)}>In Transit</Button>
      <Button onClick={() => updateStatus(3)}>Out for Delivery</Button>
      <Button 
        type="primary" 
        onClick={() => updateStatus(4)}
      >
        Mark as Delivered
      </Button>
      <Button danger onClick={() => updateStatus(5)}>Returned</Button>
    </Space>
  </Card>
)}
```

### Alternative: Webhook-Based Delivery

**Option**: Integrate with courier company's webhook API to automatically update status when delivery is confirmed.

**Implementation:**
1. Add webhook endpoint: `POST /api/webhooks/courier`
2. Receive delivery confirmation from courier service
3. Update invoice `CourierStatus` to `Delivered`
4. Send notification to patient

---

## Summary Table

| Event | Status | Trigger Point | Current Behavior | Missing |
|-------|--------|---------------|------------------|---------|
| **Prescription Ready** | ⚠️ Partial | Prescription creation | Notification sent immediately | Workflow to mark as "ready" |
| **Courier Dispatched** | ✅ Complete | Manual update via UI | Status set to Dispatched, notification sent | None |
| **Courier Delivered** | ❌ Not Implemented | N/A | No workflow exists | Complete implementation needed |

---

## Recommendations

### 1. Prescription Ready Workflow
- Add `MarkPrescriptionReadyCommand` and handler
- Add "Mark as Ready" button in prescription detail page
- Move notification trigger from creation to "ready" action
- Add permission check (only pharmacy staff can mark as ready)

### 2. Courier Delivered Workflow
- Add `UpdateCourierStatusCommand` and handler
- Add status update UI in invoice detail page
- Add `SendCourierDeliveredNotificationAsync` method in `NotificationService`
- Consider webhook integration for automatic updates

### 3. Status Tracking
- Add audit trail for status changes
- Add timestamps for each status transition
- Add user tracking (who marked as ready/dispatched/delivered)

---

## Code References

### Backend Files
- `backend/ClinicCare.Application/Features/Prescriptions/Commands/CreatePrescription/CreatePrescriptionHandler.cs`
- `backend/ClinicCare.Application/Features/Invoices/Commands/UpdateCourierDocket/UpdateCourierDocketHandler.cs`
- `backend/ClinicCare.Infrastructure/Services/NotificationService.cs`
- `backend/ClinicCare.Domain/Entities/Prescription.cs`
- `backend/ClinicCare.Domain/Entities/Invoice.cs`
- `backend/ClinicCare.Domain/Enums/UserRole.cs` (contains enums)

### Frontend Files
- `frontend/src/apps/tenant/pages/prescriptions/PrescriptionDetailPage.tsx`
- `frontend/src/apps/tenant/pages/invoices/InvoiceDetailPage.tsx`
- `frontend/src/core/services/prescriptionService.ts`
- `frontend/src/core/services/invoiceService.ts`

