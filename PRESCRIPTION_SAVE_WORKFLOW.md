# What Happens When a Prescription is Saved

This document explains the complete workflow when a prescription is saved in the prescription form page.

---

## Frontend Flow (PrescriptionFormPage.tsx)

### 1. User Action
- User fills in prescription form:
  - Adds medicines (one or more)
  - Optionally adds patient instructions/notes
- Clicks **"Create Prescription"** button (line 706)

### 2. Form Validation & Submission
```typescript
// handleSubmit function (line 374)
- Validates that at least one medicine is added
- Validates form fields
- Creates CreatePrescriptionRequest with:
  - consultationId (from URL params)
  - patientId (from URL params)
  - doctorId (current user ID)
  - medicines array
  - notes (patient instructions)
```

### 3. API Call
- Calls `prescriptionService.create(prescriptionData)`
- Uses React Query mutation (`createMutation`)

### 4. On Success (lines 117-128)

**Immediate Actions:**
1. ✅ **Success Message**: Shows "Prescription created successfully! Number: {prescriptionNumber}"
2. ✅ **Store Prescription ID**: Sets `createdPrescriptionId` state
3. ✅ **Clear Form**: 
   - Clears medicines array (`setMedicines([])`)
   - Resets form fields (`form.resetFields()`)
4. ✅ **Refresh Previous Prescriptions**: Invalidates query cache to refresh the "Previous Prescriptions" list
5. ✅ **Stay on Page**: **Does NOT navigate away** - allows user to generate invoice immediately

**UI Changes:**
- "Create Prescription" button disappears
- New buttons appear:
  - **"Generate Invoice"** button (primary, with dollar icon)
  - **"Download PDF (Internal)"** button
  - **"Back to Patient"** button

### 5. On Error
- Shows error message: "Failed to create prescription"
- User stays on form (can retry)

---

## Backend Flow (CreatePrescriptionHandler.cs)

### 1. Command Received
- `CreatePrescriptionCommand` received with:
  - ConsultationId
  - PatientId
  - DoctorId
  - Medicines array
  - Notes

### 2. Validation & Setup
- ✅ Validates organization ID
- ✅ Fetches consultation to get OrganizationId
- ✅ Generates unique prescription number

### 3. Create Prescription Entity
```csharp
Prescription prescription = new Prescription
{
    OrganizationId = consultation.OrganizationId,
    PrescriptionNumber = generatedNumber,
    ConsultationId = request.ConsultationId,
    IssuedDate = DateTime.UtcNow,
    Status = PrescriptionStatus.Issued,  // Status = 2
    PatientInstructions = request.Notes,
    PrescriptionItems = [...] // Created from medicines
}
```

**Key Points:**
- Status is set to `Issued` (2) - **NOT "Dispensed"**
- Prescription number is auto-generated
- Each medicine becomes a `PrescriptionItem`

### 4. Calculate Dispensed Quantities
For each medicine, calculates `DispensedQuantity` based on dispensing form:
- **Globules**: `quantity × containerSize × 4` (drops)
- **Tablets**: `quantity` (same as prescribed)
- **Packets**: `quantity × 5` (drops)
- **Liquid**: `quantity` (ml)
- **Tonic**: `quantity` (ml)

### 5. Save to Database
- Prescription saved to `Prescriptions` table
- PrescriptionItems saved to `PrescriptionItems` table
- Returns created prescription DTO

### 6. Stock Deduction (Async, Non-Blocking)
```csharp
// Wrapped in try-catch - doesn't fail prescription if it fails
try {
    await DeductStockFromInventoryAsync(created, cancellationToken);
} catch {
    // Log error but continue
}
```

**What Happens:**
- For each prescription item with a valid `MedicineId`:
  - Finds inventory record for that medicine in the clinic
  - Converts dispensed quantity to inventory units (ml or count)
  - Deducts from `CurrentStock`
  - Creates `StockTransaction` record for audit trail
- **If stock deduction fails**: Prescription is still created (error is logged)

### 7. Send Notification (Fire-and-Forget)
```csharp
// Line 156-166
_ = Task.Run(async () => {
    try {
        await _notificationService.SendPrescriptionReadyNotificationAsync(
            dto.Id, cancellationToken);
    } catch {
        // Ignore notification errors
    }
}, cancellationToken);
```

**Important Note:**
- ⚠️ **Notification is sent immediately** when prescription is created
- ⚠️ **This is incorrect** - should be sent when medicines are marked as "ready"
- Notification is sent asynchronously (doesn't block prescription creation)

### 8. Return Result
- Returns `Result<PrescriptionDto>` with created prescription data
- Includes prescription ID, number, medicines, etc.

---

## After Save - User Options

### Option 1: Generate Invoice
- User clicks **"Generate Invoice"** button
- Creates invoice from prescription
- Navigates to invoice detail page

### Option 2: Download PDF
- User clicks **"Download PDF (Internal)"** button
- Downloads prescription PDF with medicine names visible
- User stays on page

### Option 3: Navigate Back
- User clicks **"Back to Patient"** button
- Navigates to patient detail page

### Option 4: Stay on Page
- User can continue working
- Form is cleared and ready for next prescription (if needed)

---

## Database Changes

### Tables Updated:

1. **Prescriptions**
   - New row inserted with:
     - `Status = 2` (Issued)
     - `PrescriptionNumber` (auto-generated)
     - `IssuedDate = DateTime.UtcNow`
     - `PatientInstructions` (from form)

2. **PrescriptionItems**
   - One row per medicine
   - Includes all medicine details (dosage, frequency, duration, etc.)
   - `DispensedQuantity` calculated and stored

3. **Inventory** (if medicine has MedicineId)
   - `CurrentStock` decreased by dispensed quantity
   - `LastUpdated` set to current time

4. **StockTransactions**
   - New transaction record created for audit trail
   - `TransactionType = Sale`
   - References prescription number

---

## Key Observations

### ✅ What Works Well:
1. **Non-blocking operations**: Stock deduction and notifications don't fail prescription creation
2. **User stays on page**: Allows immediate invoice generation
3. **Form cleared**: Ready for next use
4. **Previous prescriptions refreshed**: Shows updated list

### ⚠️ Issues/Improvements Needed:

1. **Notification Timing**:
   - Currently sent when prescription is **created**
   - Should be sent when medicines are **marked as ready** (status = Dispensed)
   - **Fix**: Add workflow to mark prescription as "ready" → then send notification

2. **No Status Update Workflow**:
   - Prescription is created with status `Issued` (2)
   - No way to update to `Dispensed` (3) in current UI
   - **Fix**: Add "Mark as Ready" button for pharmacy staff

3. **Stock Deduction Errors**:
   - Errors are logged but not shown to user
   - **Fix**: Show warning if stock deduction fails

4. **Success Message**:
   - ✅ Shows prescription number
   - Could also show medicine count or other details

---

## Complete Flow Diagram

```
┌─────────────────────────────────────┐
│  User fills prescription form      │
│  - Adds medicines                  │
│  - Adds notes (optional)           │
└──────────────┬──────────────────────┘
               │
               │ Clicks "Create Prescription"
               ▼
┌─────────────────────────────────────┐
│  Frontend Validation                │
│  - Checks medicines count > 0       │
│  - Validates form fields            │
└──────────────┬──────────────────────┘
               │
               │ API Call: POST /api/prescriptions
               ▼
┌─────────────────────────────────────┐
│  Backend: CreatePrescriptionHandler │
│  1. Validate organization           │
│  2. Generate prescription number    │
│  3. Create Prescription entity      │
│  4. Create PrescriptionItems        │
│  5. Save to database                │
└──────────────┬──────────────────────┘
               │
               ├─→ Deduct Stock (async, non-blocking)
               │   - Update Inventory
               │   - Create StockTransaction
               │
               └─→ Send Notification (fire-and-forget)
                   - WhatsApp/Email/SMS
                   - "Prescription ready for collection"
               │
               ▼
┌─────────────────────────────────────┐
│  Return PrescriptionDto to Frontend │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  Frontend: onSuccess                │
│  1. Show success message           │
│  2. Store prescription ID           │
│  3. Clear form & medicines          │
│  4. Refresh previous prescriptions │
│  5. Show new buttons:               │
│     - Generate Invoice              │
│     - Download PDF                  │
│     - Back to Patient               │
└─────────────────────────────────────┘
```

---

## Code References

### Frontend Files:
- `frontend/src/apps/tenant/pages/prescriptions/PrescriptionFormPage.tsx`
  - Line 374: `handleSubmit` function
  - Line 115-132: `createMutation` with success/error handlers
  - Line 696-754: UI buttons after save

### Backend Files:
- `backend/ClinicCare.Application/Features/Prescriptions/Commands/CreatePrescription/CreatePrescriptionHandler.cs`
  - Line 45: `Handle` method
  - Line 136: Save prescription
  - Line 143-153: Stock deduction (non-blocking)
  - Line 156-166: Notification (fire-and-forget)

### Database Tables:
- `Prescriptions`
- `PrescriptionItems`
- `Inventory`
- `StockTransactions`

---

## Summary

When a prescription is saved:

1. ✅ **Prescription is created** with status "Issued"
2. ✅ **Stock is deducted** from inventory (if available)
3. ✅ **Notification is sent** (immediately - this is the issue)
4. ✅ **Form is cleared** and user stays on page
5. ✅ **User can generate invoice** immediately
6. ⚠️ **No way to mark as "ready"** - missing workflow

**Main Issue**: Notification is sent too early (on creation instead of when ready).

