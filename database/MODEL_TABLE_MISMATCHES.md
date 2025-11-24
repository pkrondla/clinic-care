# Entity Models vs Database Tables - Mismatch Report

## ✅ Correctly Mapped Entities

### Global Database
- ✅ **Organizations** - All properties match
- ✅ **SystemUsers** - All properties match
- ✅ **SubscriptionPlans** - All properties match
- ✅ **OrganizationSubscriptions** - All properties match (IsActive ignored, uses Status)
- ✅ **GlobalMedicines** - All properties match (Price → MRP column)
- ✅ **PaymentTransactions** - All properties match
- ✅ **AuditLogs** - All properties match

### Tenant Database
- ✅ **Users** - Fixed (added OrganizationId, RefreshToken, RefreshTokenExpiryTime)
- ✅ **DoctorProfiles** - Fixed (added OrganizationId, IsActive)
- ✅ **UserClinicAccess** - All properties match
- ✅ **PrescriptionMedicines** (PrescriptionItem) - All properties match
- ✅ **Medicines** (ClinicMedicine) - All properties match
- ✅ **Appointments** - All properties match (OrganizationId inherited from TenantEntity)

---

## ⚠️ Mismatched Entities

### 1. **Clinic** Entity
**Table**: `Clinics`
**Status**: Missing properties in model

**Missing in Model**:
- `City` (nvarchar, nullable)
- `State` (nvarchar, nullable)
- `PinCode` (nvarchar, nullable)
- `WorkingHours` (nvarchar, nullable)

**Action**: Add these properties to `Clinic.cs` entity

---

### 2. **Patient** Entity
**Table**: `Patients`
**Status**: Significant mismatch

**Model Has (but DB doesn't)**:
- `UserId` - Database doesn't have this column

**Database Has (but Model doesn't)**:
- `FirstName` (nvarchar, NOT NULL)
- `LastName` (nvarchar, NOT NULL)
- `Phone` (nvarchar, NOT NULL)
- `Email` (nvarchar, nullable)
- `City` (nvarchar, nullable)
- `State` (nvarchar, nullable)
- `PinCode` (nvarchar, nullable)
- `EmergencyContactName` (nvarchar, nullable) - Model has `EmergencyContact`
- `EmergencyContactPhone` (nvarchar, nullable)
- `Allergies` (nvarchar, nullable)
- `CurrentMedications` (nvarchar, nullable)

**Action Required**: 
- Decide: Should Patient have UserId or store FirstName/LastName directly?
- Add missing properties to model or remove from database

---

### 3. **Consultation** Entity
**Table**: `Consultations`
**Status**: Significant mismatch

**Model Has (but DB doesn't)**:
- `DoctorId` - Should come from Appointment
- `PatientId` - Should come from Appointment
- `Examination` - Database has `Observations` instead
- `Notes` - Database has `DoctorNotes` instead
- `ConsultationFee` - Not in database
- `ConsultationDate` - Not in database

**Database Has (but Model doesn't)**:
- `Observations` (nvarchar, nullable) - Model has `Examination`
- `DoctorNotes` (nvarchar, nullable) - Model has `Notes`
- `VitalSigns` (nvarchar, nullable)
- `Duration` (int, nullable)
- `ConsultationType` (int, NOT NULL)

**Action Required**: 
- Align property names (Examination → Observations, Notes → DoctorNotes)
- Add missing properties (VitalSigns, Duration, ConsultationType)
- Remove DoctorId/PatientId (get from Appointment navigation)
- Decide on ConsultationFee/ConsultationDate location

---

### 4. **Prescription** Entity
**Table**: `Prescriptions`
**Status**: Property name mismatches

**Model Has (but DB doesn't)**:
- `Status` (PrescriptionStatus enum)
- `InternalNotes`
- `PatientInstructions`
- `IssuedDate`

**Database Has (but Model doesn't)**:
- `Instructions` (nvarchar, nullable) - Model has `PatientInstructions` and `InternalNotes`
- `DietAdvice` (nvarchar, nullable)
- `LifestyleAdvice` (nvarchar, nullable)
- `FollowupDate` (date, nullable)
- `FollowupNotes` (nvarchar, nullable)

**Action Required**: 
- Map `PatientInstructions` → `Instructions` or split `Instructions` into two fields
- Add `DietAdvice`, `LifestyleAdvice`, `FollowupDate`, `FollowupNotes`
- Add `Status` column to database or remove from model
- Add `IssuedDate` column or use `CreatedAt`

---

### 5. **Inventory** Entity
**Table**: `Inventory`
**Status**: Missing pricing columns

**Model Has (but DB doesn't)**:
- `PurchasePrice` (decimal)
- `SellingPrice` (decimal)
- `LastUpdated` (DateTime)

**Database Has (but Model doesn't)**:
- `ReorderLevel` (int, NOT NULL)
- `LastRestockDate` (datetime2, nullable)

**Action Required**: 
- Add `PurchasePrice` and `SellingPrice` columns to database
- Add `ReorderLevel` property to model
- Map `LastUpdated` → `LastRestockDate` or add both

---

### 6. **Invoice** Entity
**Table**: `Invoices`
**Status**: Significant structural differences

**Model Has (but DB doesn't)**:
- `ConsultationId` - Database has `AppointmentId` instead
- `ConsultationAmount` - Database has `ConsultationCharges`
- `MedicineAmount` - Database has `MedicineCharges`
- `PaidAmount` (decimal)
- `BalanceAmount` (decimal)
- `Status` (InvoiceStatus enum) - Database has `PaymentStatus` (int)
- `PaymentReference` (string)
- `InvoiceDate` (DateTime)

**Database Has (but Model doesn't)**:
- `AppointmentId` (int, NOT NULL) - Model has `ConsultationId`
- `ConsultationCharges` - Model has `ConsultationAmount`
- `MedicineCharges` - Model has `MedicineAmount`
- `Discount` (decimal, NOT NULL)
- `TaxAmount` (decimal, NOT NULL)
- `FinalAmount` (decimal, NOT NULL) - Model calculates from TotalAmount
- `PaymentStatus` (int, NOT NULL) - Model has `Status` enum
- `Notes` (nvarchar, nullable)

**Action Required**: 
- Align property names (ConsultationAmount → ConsultationCharges, etc.)
- Add `Discount`, `TaxAmount`, `Notes` to model
- Decide on `AppointmentId` vs `ConsultationId`
- Add `PaidAmount`, `BalanceAmount`, `PaymentReference`, `InvoiceDate` to database or remove from model
- Align `Status` enum with `PaymentStatus` int

---

## ❌ Missing Tables

### 1. **DoctorAvailabilities** Table
**Entity**: `DoctorAvailability` (TenantEntity)
**Status**: Table doesn't exist in database
**Action**: Create table

### 2. **StockTransactions** Table
**Entity**: `StockTransaction` (TenantEntity)
**Status**: Table doesn't exist in database
**Action**: Create table

### 3. **InvoiceItems** Table
**Entity**: `InvoiceItem` (TenantEntity)
**Status**: Table doesn't exist in database
**Action**: Create table

### 4. **Communications** Table
**Entity**: `Communication` (TenantEntity)
**Status**: Table doesn't exist in database
**Action**: Create table

---

## 📋 Priority Fix List

### High Priority (Blocking Functionality)
1. **Create missing tables**: DoctorAvailabilities, StockTransactions, InvoiceItems, Communications
2. **Fix Consultation entity**: Align with database schema
3. **Fix Invoice entity**: Align property names and structure

### Medium Priority (Data Integrity)
4. **Fix Patient entity**: Decide on UserId vs direct fields
5. **Fix Prescription entity**: Align property names
6. **Fix Inventory entity**: Add pricing columns or remove from model

### Low Priority (Enhancement)
7. **Add Clinic properties**: City, State, PinCode, WorkingHours

---

## 🔧 Quick Fixes Applied

✅ **Users table**: Added OrganizationId, RefreshToken, RefreshTokenExpiryTime
✅ **DoctorProfiles table**: Added OrganizationId, IsActive

---

## 📝 Notes

- All entities inherit from `BaseEntity` (Id, CreatedAt, UpdatedAt, IsActive)
- Tenant entities inherit from `TenantEntity` (adds OrganizationId)
- Navigation properties (Organization, User, etc.) are not database columns
- Computed properties (FullName, Age, etc.) are not database columns


