-- ClinicCare Database Indexes Creation Script
-- Performance optimization indexes for multi-tenant queries

USE ClinicCareDb;
GO

-- =====================================
-- Performance Indexes (Tenant-Scoped)
-- =====================================

-- Organizations
CREATE INDEX IX_Organizations_Subdomain_Active ON Organizations (Subdomain, IsActive);
CREATE INDEX IX_Organizations_Active ON Organizations (IsActive);

-- Clinics
CREATE INDEX IX_Clinics_Organization_Active ON Clinics (OrganizationId, IsActive);
CREATE INDEX IX_Clinics_Code ON Clinics (OrganizationId, Code);

-- Users
CREATE INDEX IX_Users_Organization_Role ON Users (OrganizationId, Role, IsActive);
CREATE INDEX IX_Users_Organization_Active ON Users (OrganizationId, IsActive);

-- UserOrganizations
CREATE INDEX IX_UserOrganizations_User_Active ON UserOrganizations (UserId, IsActive);
CREATE INDEX IX_UserOrganizations_Organization_Role ON UserOrganizations (OrganizationId, Role, IsActive);

-- DoctorProfiles
CREATE INDEX IX_DoctorProfiles_Organization_Active ON DoctorProfiles (OrganizationId, IsActive);
CREATE INDEX IX_DoctorProfiles_User ON DoctorProfiles (UserId);

-- DoctorAvailabilities
CREATE INDEX IX_DoctorAvailabilities_Organization_Doctor_Date ON DoctorAvailabilities (OrganizationId, DoctorId, AvailableDate);
CREATE INDEX IX_DoctorAvailabilities_Organization_Clinic_Date ON DoctorAvailabilities (OrganizationId, ClinicId, AvailableDate);
CREATE INDEX IX_DoctorAvailabilities_Date_Active ON DoctorAvailabilities (AvailableDate, IsActive);

-- Patients
CREATE INDEX IX_Patients_Organization_Code ON Patients (OrganizationId, PatientCode);
CREATE INDEX IX_Patients_Organization_Active ON Patients (OrganizationId, IsActive);
CREATE INDEX IX_Patients_User ON Patients (UserId);

-- Appointments (Critical for queue management)
CREATE INDEX IX_Appointments_Organization_Doctor_Date ON Appointments (OrganizationId, DoctorId, AppointmentDate);
CREATE INDEX IX_Appointments_Organization_Clinic_Date ON Appointments (OrganizationId, ClinicId, AppointmentDate);
CREATE INDEX IX_Appointments_Organization_Patient ON Appointments (OrganizationId, PatientId);
CREATE INDEX IX_Appointments_Date_Status ON Appointments (AppointmentDate, Status, IsActive);
CREATE INDEX IX_Appointments_Queue_Lookup ON Appointments (OrganizationId, ClinicId, DoctorId, AppointmentDate, TokenNumber);

-- Consultations
CREATE INDEX IX_Consultations_Organization_Doctor ON Consultations (OrganizationId, DoctorId);
CREATE INDEX IX_Consultations_Organization_Patient ON Consultations (OrganizationId, PatientId);
CREATE INDEX IX_Consultations_Date ON Consultations (ConsultationDate);
CREATE INDEX IX_Consultations_Appointment ON Consultations (AppointmentId);

-- Prescriptions
CREATE INDEX IX_Prescriptions_Organization_Status ON Prescriptions (OrganizationId, Status);
CREATE INDEX IX_Prescriptions_Organization_Number ON Prescriptions (OrganizationId, PrescriptionNumber);
CREATE INDEX IX_Prescriptions_Consultation ON Prescriptions (ConsultationId);
CREATE INDEX IX_Prescriptions_IssuedDate ON Prescriptions (IssuedDate);

-- PrescriptionItems
CREATE INDEX IX_PrescriptionItems_Organization_Prescription ON PrescriptionItems (OrganizationId, PrescriptionId);
CREATE INDEX IX_PrescriptionItems_Medicine ON PrescriptionItems (MedicineId);

-- ClinicMedicines
CREATE INDEX IX_ClinicMedicines_Organization_Clinic ON ClinicMedicines (OrganizationId, ClinicId, IsActive);
CREATE INDEX IX_ClinicMedicines_GlobalMedicine ON ClinicMedicines (GlobalMedicineId);
CREATE INDEX IX_ClinicMedicines_Name_Search ON ClinicMedicines (OrganizationId, Name, IsActive);

-- Inventories (Critical for stock management)
CREATE INDEX IX_Inventories_Organization_Clinic ON Inventories (OrganizationId, ClinicId, IsActive);
CREATE INDEX IX_Inventories_Medicine ON Inventories (MedicineId);
CREATE INDEX IX_Inventories_LowStock ON Inventories (OrganizationId, ClinicId, CurrentStock, MinimumStock) WHERE IsActive = 1;
CREATE INDEX IX_Inventories_Expiry ON Inventories (OrganizationId, ExpiryDate) WHERE IsActive = 1;
CREATE INDEX IX_Inventories_LastUpdated ON Inventories (LastUpdated);

-- StockTransactions
CREATE INDEX IX_StockTransactions_Organization_Clinic ON StockTransactions (OrganizationId, ClinicId);
CREATE INDEX IX_StockTransactions_Medicine_Date ON StockTransactions (MedicineId, TransactionDate);
CREATE INDEX IX_StockTransactions_Type_Date ON StockTransactions (TransactionType, TransactionDate);
CREATE INDEX IX_StockTransactions_Transfer ON StockTransactions (FromClinicId, ToClinicId, TransactionDate);

-- Invoices
CREATE INDEX IX_Invoices_Organization_Patient ON Invoices (OrganizationId, PatientId);
CREATE INDEX IX_Invoices_Organization_Status ON Invoices (OrganizationId, Status);
CREATE INDEX IX_Invoices_Organization_Number ON Invoices (OrganizationId, InvoiceNumber);
CREATE INDEX IX_Invoices_Date_Status ON Invoices (InvoiceDate, Status);
CREATE INDEX IX_Invoices_Clinic_Date ON Invoices (ClinicId, InvoiceDate);

-- InvoiceItems
CREATE INDEX IX_InvoiceItems_Organization_Invoice ON InvoiceItems (OrganizationId, InvoiceId);
CREATE INDEX IX_InvoiceItems_Type ON InvoiceItems (ItemType);

-- Communications
CREATE INDEX IX_Communications_Organization_Patient ON Communications (OrganizationId, PatientId);
CREATE INDEX IX_Communications_Type_Status ON Communications (Type, Status);
CREATE INDEX IX_Communications_ScheduledAt ON Communications (ScheduledAt) WHERE ScheduledAt IS NOT NULL;
CREATE INDEX IX_Communications_Status_Pending ON Communications (Status, ScheduledAt) WHERE Status = 1; -- Pending

-- GlobalMedicines
CREATE INDEX IX_GlobalMedicines_Name_Active ON GlobalMedicines (Name, IsActive);
CREATE INDEX IX_GlobalMedicines_Type_Active ON GlobalMedicines (Type, IsActive);
CREATE INDEX IX_GlobalMedicines_Search ON GlobalMedicines (Name, GenericName, Manufacturer, IsActive);

-- =====================================
-- Covering Indexes for Common Queries
-- =====================================

-- User login lookup
CREATE INDEX IX_Users_Login_Lookup ON Users (OrganizationId, Email, IsActive) 
INCLUDE (Id, PasswordHash, FirstName, LastName, Role);

-- Appointment queue lookup
CREATE INDEX IX_Appointments_Queue_Display ON Appointments (OrganizationId, DoctorId, ClinicId, AppointmentDate, Status) 
INCLUDE (Id, TokenNumber, Type, PatientId) WHERE IsActive = 1;

-- Doctor availability lookup
CREATE INDEX IX_DoctorAvailabilities_Lookup ON DoctorAvailabilities (OrganizationId, ClinicId, AvailableDate, IsActive) 
INCLUDE (DoctorId, StartTime, EndTime);

-- Patient appointment history
CREATE INDEX IX_Appointments_Patient_History ON Appointments (OrganizationId, PatientId, AppointmentDate DESC) 
INCLUDE (Id, DoctorId, ClinicId, Status, Type) WHERE IsActive = 1;

-- Medicine inventory lookup
CREATE INDEX IX_Inventories_Medicine_Lookup ON Inventories (OrganizationId, ClinicId, MedicineId) 
INCLUDE (CurrentStock, MinimumStock, SellingPrice, ExpiryDate) WHERE IsActive = 1;

-- Prescription dispensing lookup
CREATE INDEX IX_Prescriptions_Dispensing ON Prescriptions (OrganizationId, Status, IssuedDate) 
INCLUDE (Id, PrescriptionNumber, ConsultationId) WHERE IsActive = 1;

GO

-- =====================================
-- Statistics Update
-- =====================================

-- Update statistics for all tables to ensure optimal query performance
UPDATE STATISTICS Organizations;
UPDATE STATISTICS Clinics;
UPDATE STATISTICS Users;
UPDATE STATISTICS UserOrganizations;
UPDATE STATISTICS DoctorProfiles;
UPDATE STATISTICS DoctorAvailabilities;
UPDATE STATISTICS Patients;
UPDATE STATISTICS Appointments;
UPDATE STATISTICS Consultations;
UPDATE STATISTICS Prescriptions;
UPDATE STATISTICS PrescriptionItems;
UPDATE STATISTICS ClinicMedicines;
UPDATE STATISTICS Inventories;
UPDATE STATISTICS StockTransactions;
UPDATE STATISTICS Invoices;
UPDATE STATISTICS InvoiceItems;
UPDATE STATISTICS Communications;
UPDATE STATISTICS GlobalMedicines;

GO
