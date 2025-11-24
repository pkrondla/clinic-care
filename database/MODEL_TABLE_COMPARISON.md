# Entity Models vs Database Tables Comparison

This document compares all entity models with their corresponding database tables to identify mismatches.

## Global Database (ClinicCare_Global)

### Organizations Table
**Entity**: `Organization` (BaseEntity)
**Status**: ✅ Match
- All properties mapped correctly
- Includes: Id, Name, Subdomain, DatabaseName, ContactEmail, ContactPhone, Address, SubscriptionStatus, TrialEndDate, IsActive, CreatedAt, UpdatedAt

### SystemUsers Table
**Entity**: `SystemUser` (BaseEntity)
**Status**: ✅ Match
- All properties mapped correctly
- Includes: Id, Email, PasswordHash, FirstName, LastName, Phone, Role, IsActive, LastLoginAt, CreatedAt, UpdatedAt

### SubscriptionPlans Table
**Entity**: `SubscriptionPlan` (BaseEntity)
**Status**: ✅ Match
- All properties mapped correctly

### OrganizationSubscriptions Table
**Entity**: `OrganizationSubscription` (BaseEntity)
**Status**: ✅ Match
- Note: IsActive is ignored (uses Status field instead)

### GlobalMedicines Table
**Entity**: `GlobalMedicine` (BaseEntity)
**Status**: ✅ Match
- Price property maps to MRP column
- All properties mapped correctly

### PaymentTransactions Table
**Entity**: `PaymentTransaction` (BaseEntity)
**Status**: ✅ Match
- All properties mapped correctly

### AuditLogs Table
**Entity**: `AuditLog` (BaseEntity)
**Status**: ✅ Match
- All properties mapped correctly

## Tenant Database (ClinicCare_demo)

### Users Table
**Entity**: `User` (TenantEntity)
**Status**: ✅ Fixed
- **Previously Missing**: OrganizationId, RefreshToken, RefreshTokenExpiryTime
- **Fixed**: Added via migration script `017_UpdateUsersTable.sql`
- All properties now mapped correctly

### Clinics Table
**Entity**: `Clinic` (TenantEntity)
**Status**: ⚠️ Check Required
- **Model Properties**: Name, Code, Address, ContactPhone, ContactEmail
- **Database Has**: Name, Code, Address, City, State, PinCode, ContactPhone, ContactEmail, WorkingHours, IsActive, CreatedAt, UpdatedAt
- **Missing in Model**: City, State, PinCode, WorkingHours
- **Note**: Model inherits OrganizationId, IsActive, CreatedAt, UpdatedAt from TenantEntity

### UserClinicAccess Table
**Entity**: `UserClinicAccess` (BaseEntity)
**Status**: ✅ Match
- All properties mapped correctly

### DoctorProfiles Table
**Entity**: `DoctorProfile` (TenantEntity)
**Status**: ✅ Fixed
- **Previously Missing**: OrganizationId, IsActive
- **Fixed**: Added via migration script `018_UpdateDoctorProfilesTable.sql`
- **Model Has**: Bio property (not in database)
- **Database Has**: Bio column (nvarchar, nullable)
- All properties now mapped correctly

### DoctorAvailabilities Table
**Entity**: `DoctorAvailability` (TenantEntity)
**Status**: ⚠️ Table Missing
- **Issue**: Table doesn't exist in database (0 rows returned)
- **Action Required**: Create table

### Patients Table
**Entity**: `Patient` (TenantEntity)
**Status**: ⚠️ Mismatch
- **Model Properties**: UserId, PatientCode, DateOfBirth, Gender, BloodGroup, Address, EmergencyContact, MedicalHistory
- **Database Has**: PatientCode, FirstName, LastName, DateOfBirth, Gender, Phone, Email, Address, City, State, PinCode, BloodGroup, EmergencyContactName, EmergencyContactPhone, MedicalHistory, Allergies, CurrentMedications, IsActive, CreatedAt, UpdatedAt
- **Missing in Model**: FirstName, LastName, Phone, Email, City, State, PinCode, EmergencyContactName, EmergencyContactPhone, Allergies, CurrentMedications
- **Missing in Database**: UserId (model expects it)
- **Note**: Model has User navigation property but database doesn't have UserId column

### Appointments Table
**Entity**: `Appointment` (TenantEntity)
**Status**: ⚠️ Check Required
- **Model Properties**: ClinicId, DoctorId, PatientId, AppointmentDate, TokenNumber, Type, Status, IsFollowup, Notes, CancellationReason
- **Database Has**: ClinicId, DoctorId, PatientId, AppointmentDate, TokenNumber, Type, Status, IsFollowup, Notes, CancellationReason, CreatedAt, UpdatedAt
- **Missing in Model**: OrganizationId (inherited from TenantEntity)
- **Status**: ✅ Match (OrganizationId is inherited)

### Consultations Table
**Entity**: `Consultation` (TenantEntity)
**Status**: ⚠️ Mismatch
- **Model Properties**: AppointmentId, DoctorId, PatientId, ChiefComplaint, Symptoms, Examination, Diagnosis, TreatmentPlan, Notes, ConsultationFee, ConsultationDate
- **Database Has**: AppointmentId, ChiefComplaint, Symptoms, Diagnosis, TreatmentPlan, Observations, DoctorNotes, VitalSigns, Duration, ConsultationType, CreatedAt, UpdatedAt
- **Missing in Model**: Observations, DoctorNotes, VitalSigns, Duration, ConsultationType
- **Missing in Database**: DoctorId, PatientId, Examination, Notes, ConsultationFee, ConsultationDate
- **Note**: DoctorId and PatientId should come from Appointment, but model has them directly

### Prescriptions Table
**Entity**: `Prescription` (TenantEntity)
**Status**: ⚠️ Mismatch
- **Model Properties**: ConsultationId, PrescriptionNumber, IssuedDate, PatientInstructions, InternalNotes, Status
- **Database Has**: ConsultationId, PrescriptionNumber, Instructions, DietAdvice, LifestyleAdvice, FollowupDate, FollowupNotes, CreatedAt, UpdatedAt
- **Missing in Model**: Instructions, DietAdvice, LifestyleAdvice, FollowupDate, FollowupNotes
- **Missing in Database**: IssuedDate, PatientInstructions, InternalNotes, Status

### PrescriptionMedicines Table
**Entity**: `PrescriptionItem` (TenantEntity)
**Status**: ✅ Match
- All properties mapped correctly

### Medicines Table
**Entity**: `ClinicMedicine` (TenantEntity)
**Status**: ✅ Match
- All properties mapped correctly
- Note: Table name is "Medicines" but entity is "ClinicMedicine"

### Inventory Table
**Entity**: `Inventory` (TenantEntity)
**Status**: ⚠️ Mismatch
- **Model Properties**: ClinicId, MedicineId, CurrentStock, MinimumStock, MaximumStock, PurchasePrice, SellingPrice, ExpiryDate, BatchNumber, LastUpdated
- **Database Has**: ClinicId, MedicineId, CurrentStock, MinimumStock, MaximumStock, ReorderLevel, LastRestockDate, ExpiryDate, BatchNumber, CreatedAt, UpdatedAt
- **Missing in Model**: ReorderLevel, LastRestockDate
- **Missing in Database**: PurchasePrice, SellingPrice, LastUpdated
- **Note**: PurchasePrice and SellingPrice might be in StockTransactions or Medicines table

### StockTransactions Table
**Entity**: `StockTransaction` (TenantEntity)
**Status**: ⚠️ Table Missing
- **Issue**: Table doesn't exist in database (0 rows returned)
- **Action Required**: Create table

### Invoices Table
**Entity**: `Invoice` (TenantEntity)
**Status**: ⚠️ Mismatch
- **Model Properties**: ClinicId, PatientId, ConsultationId, InvoiceNumber, ConsultationAmount, MedicineAmount, CourierCharges, TotalAmount, PaidAmount, BalanceAmount, Status, PaymentMethod, PaymentReference, InvoiceDate, PaymentDate
- **Database Has**: InvoiceNumber, AppointmentId, PatientId, ClinicId, ConsultationCharges, MedicineCharges, CourierCharges, TotalAmount, Discount, TaxAmount, FinalAmount, PaymentStatus, PaymentMethod, PaymentDate, Notes, CreatedAt, UpdatedAt
- **Missing in Model**: AppointmentId, Discount, TaxAmount, FinalAmount, Notes
- **Missing in Database**: ConsultationId, PaidAmount, BalanceAmount, Status (uses PaymentStatus), PaymentReference, InvoiceDate
- **Note**: Model uses ConsultationId but database uses AppointmentId

### InvoiceItems Table
**Entity**: `InvoiceItem` (TenantEntity)
**Status**: ⚠️ Table Missing
- **Issue**: Table doesn't exist in database (0 rows returned)
- **Action Required**: Create table

### Communications Table
**Entity**: `Communication` (TenantEntity)
**Status**: ⚠️ Table Missing
- **Issue**: Table doesn't exist in database (0 rows returned)
- **Action Required**: Create table

## Summary of Issues

### Critical Issues (Blocking)
1. **DoctorAvailabilities** - Table doesn't exist
2. **StockTransactions** - Table doesn't exist
3. **InvoiceItems** - Table doesn't exist
4. **Communications** - Table doesn't exist

### Schema Mismatches
1. **Patient** - Missing UserId in database, missing several fields in model
2. **Consultation** - Significant property mismatches
3. **Prescription** - Property name mismatches
4. **Inventory** - Missing PurchasePrice/SellingPrice in database
5. **Invoice** - Different property names and structure

### Recommendations
1. Create missing tables (DoctorAvailabilities, StockTransactions, InvoiceItems, Communications)
2. Align Patient entity with database schema (add missing fields or remove UserId)
3. Review and align Consultation, Prescription, Invoice entities with database
4. Decide on Inventory pricing strategy (where to store PurchasePrice/SellingPrice)


