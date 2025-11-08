-- ClinicCare Database Schema Creation Script
-- Multi-tenant Homoeopathy Clinic Management System

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ClinicCareDb')
BEGIN
    CREATE DATABASE ClinicCareDb;
END
GO

USE ClinicCareDb;
GO

-- ====================================
-- Global Tables (No Tenant Isolation)
-- ====================================

-- Global Medicines (Shared across all tenants)
CREATE TABLE GlobalMedicines (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    GenericName NVARCHAR(200) NOT NULL,
    Type NVARCHAR(100) NOT NULL,
    Potency NVARCHAR(50) NOT NULL,
    Manufacturer NVARCHAR(200) NOT NULL,
    Price DECIMAL(10,2) NOT NULL DEFAULT 0,
    Description NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ========================================
-- Multi-Tenant Tables (Include TenantId)
-- ========================================

-- Organizations (Root tenant entity)
CREATE TABLE Organizations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Subdomain NVARCHAR(100) NOT NULL,
    ContactEmail NVARCHAR(255) NOT NULL,
    ContactPhone NVARCHAR(20) NULL,
    Address NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT UK_Organizations_Subdomain UNIQUE (Subdomain)
);

-- Clinics (Physical locations)
CREATE TABLE Clinics (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Address NVARCHAR(500) NULL,
    ContactPhone NVARCHAR(20) NULL,
    ContactEmail NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Clinics_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT UK_Clinics_Code_Org UNIQUE (OrganizationId, Code)
);

-- Users (Multi-role users)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Role INT NOT NULL, -- 1=SuperAdmin, 2=Admin, 3=Doctor, 4=Staff, 5=Patient
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Users_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT UK_Users_Email_Org UNIQUE (OrganizationId, Email)
);

-- User Organization Access (Cross-organization mapping)
CREATE TABLE UserOrganizations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    OrganizationId INT NOT NULL,
    Role INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_UserOrganizations_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserOrganizations_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT UK_UserOrganizations_User_Org UNIQUE (UserId, OrganizationId)
);

-- Doctor Profiles
CREATE TABLE DoctorProfiles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    UserId INT NOT NULL,
    RegistrationNumber NVARCHAR(100) NOT NULL,
    Qualification NVARCHAR(500) NOT NULL,
    ExperienceYears INT NOT NULL DEFAULT 0,
    Specialization NVARCHAR(200) NULL,
    ConsultationFeeInPerson DECIMAL(10,2) NOT NULL DEFAULT 0,
    ConsultationFeeTele DECIMAL(10,2) NOT NULL DEFAULT 0,
    FollowupFeeInPerson DECIMAL(10,2) NOT NULL DEFAULT 0,
    FollowupFeeTele DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_DoctorProfiles_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_DoctorProfiles_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UK_DoctorProfiles_User UNIQUE (UserId)
);

-- Doctor Availability
CREATE TABLE DoctorAvailabilities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    DoctorId INT NOT NULL,
    ClinicId INT NOT NULL,
    AvailableDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_DoctorAvailabilities_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_DoctorAvailabilities_Doctor FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id),
    CONSTRAINT FK_DoctorAvailabilities_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id)
);

-- Patients
CREATE TABLE Patients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    UserId INT NOT NULL,
    PatientCode NVARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    BloodGroup NVARCHAR(10) NULL,
    Address NVARCHAR(500) NULL,
    EmergencyContact NVARCHAR(20) NULL,
    MedicalHistory NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Patients_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Patients_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UK_Patients_Code_Org UNIQUE (OrganizationId, PatientCode),
    CONSTRAINT UK_Patients_User UNIQUE (UserId)
);

-- Appointments (Token-based system)
CREATE TABLE Appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    ClinicId INT NOT NULL,
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    TokenNumber INT NOT NULL,
    Type INT NOT NULL, -- 1=InPerson, 2=Teleconsultation
    Status INT NOT NULL, -- 1=Scheduled, 2=InProgress, 3=Completed, 4=Cancelled
    Notes NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Appointments_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Appointments_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_Appointments_Doctor FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id),
    CONSTRAINT FK_Appointments_Patient FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    CONSTRAINT UK_Appointments_Token UNIQUE (OrganizationId, ClinicId, DoctorId, AppointmentDate, TokenNumber)
);

-- Consultations
CREATE TABLE Consultations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    AppointmentId INT NOT NULL,
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    ChiefComplaint NVARCHAR(1000) NULL,
    Symptoms NVARCHAR(MAX) NULL,
    Examination NVARCHAR(MAX) NULL,
    Diagnosis NVARCHAR(MAX) NULL,
    TreatmentPlan NVARCHAR(MAX) NULL,
    Notes NVARCHAR(MAX) NULL,
    ConsultationFee DECIMAL(10,2) NOT NULL,
    ConsultationDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Consultations_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Consultations_Appointment FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
    CONSTRAINT FK_Consultations_Doctor FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id),
    CONSTRAINT FK_Consultations_Patient FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    CONSTRAINT UK_Consultations_Appointment UNIQUE (AppointmentId)
);

-- Prescriptions
CREATE TABLE Prescriptions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    ConsultationId INT NOT NULL,
    PrescriptionNumber NVARCHAR(50) NOT NULL,
    Status INT NOT NULL, -- 1=Draft, 2=Issued, 3=Dispensed
    InternalNotes NVARCHAR(MAX) NULL,
    PatientInstructions NVARCHAR(MAX) NULL,
    IssuedDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Prescriptions_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Prescriptions_Consultation FOREIGN KEY (ConsultationId) REFERENCES Consultations(Id),
    CONSTRAINT UK_Prescriptions_Number_Org UNIQUE (OrganizationId, PrescriptionNumber)
);

-- Clinic Medicines (Organization-specific medicines)
CREATE TABLE ClinicMedicines (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    ClinicId INT NOT NULL,
    GlobalMedicineId INT NULL,
    Name NVARCHAR(200) NOT NULL,
    GenericName NVARCHAR(200) NOT NULL,
    Type NVARCHAR(100) NOT NULL,
    Potency NVARCHAR(50) NOT NULL,
    Manufacturer NVARCHAR(200) NOT NULL,
    PurchasePrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    SellingPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    Description NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ClinicMedicines_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_ClinicMedicines_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_ClinicMedicines_GlobalMedicine FOREIGN KEY (GlobalMedicineId) REFERENCES GlobalMedicines(Id)
);

-- Prescription Items
CREATE TABLE PrescriptionItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    PrescriptionId INT NOT NULL,
    MedicineId INT NOT NULL,
    MedicineName NVARCHAR(200) NOT NULL,
    Dosage NVARCHAR(100) NOT NULL,
    Frequency NVARCHAR(100) NOT NULL,
    Duration NVARCHAR(100) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    Instructions NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_PrescriptionItems_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_PrescriptionItems_Prescription FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id),
    CONSTRAINT FK_PrescriptionItems_Medicine FOREIGN KEY (MedicineId) REFERENCES ClinicMedicines(Id)
);

-- Inventory (Per-clinic stock tracking)
CREATE TABLE Inventories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    ClinicId INT NOT NULL,
    MedicineId INT NOT NULL,
    CurrentStock INT NOT NULL DEFAULT 0,
    MinimumStock INT NOT NULL DEFAULT 0,
    MaximumStock INT NOT NULL DEFAULT 0,
    PurchasePrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    SellingPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    ExpiryDate DATE NOT NULL,
    BatchNumber NVARCHAR(50) NULL,
    LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Inventories_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Inventories_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_Inventories_Medicine FOREIGN KEY (MedicineId) REFERENCES ClinicMedicines(Id),
    CONSTRAINT UK_Inventories_Clinic_Medicine UNIQUE (ClinicId, MedicineId, BatchNumber)
);

-- Stock Transactions
CREATE TABLE StockTransactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    ClinicId INT NOT NULL,
    MedicineId INT NOT NULL,
    TransactionType INT NOT NULL, -- 1=Purchase, 2=Sale, 3=Transfer, 4=Adjustment
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Reference NVARCHAR(100) NULL,
    Notes NVARCHAR(500) NULL,
    FromClinicId INT NULL,
    ToClinicId INT NULL,
    TransactionDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_StockTransactions_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_StockTransactions_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_StockTransactions_Medicine FOREIGN KEY (MedicineId) REFERENCES ClinicMedicines(Id),
    CONSTRAINT FK_StockTransactions_FromClinic FOREIGN KEY (FromClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_StockTransactions_ToClinic FOREIGN KEY (ToClinicId) REFERENCES Clinics(Id)
);

-- Invoices
CREATE TABLE Invoices (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    ClinicId INT NOT NULL,
    PatientId INT NOT NULL,
    ConsultationId INT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    ConsultationAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    MedicineAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    CourierCharges DECIMAL(10,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    PaidAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    BalanceAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    Status INT NOT NULL, -- 1=Draft, 2=Sent, 3=Paid, 4=Cancelled
    PaymentMethod NVARCHAR(50) NULL,
    PaymentReference NVARCHAR(100) NULL,
    InvoiceDate DATETIME2 NOT NULL,
    PaymentDate DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Invoices_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Invoices_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_Invoices_Patient FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    CONSTRAINT FK_Invoices_Consultation FOREIGN KEY (ConsultationId) REFERENCES Consultations(Id),
    CONSTRAINT UK_Invoices_Number_Org UNIQUE (OrganizationId, InvoiceNumber)
);

-- Invoice Items
CREATE TABLE InvoiceItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    InvoiceId INT NOT NULL,
    ItemType NVARCHAR(50) NOT NULL, -- Consultation, Medicine, Courier
    Description NVARCHAR(200) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_InvoiceItems_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_InvoiceItems_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id)
);

-- Communications
CREATE TABLE Communications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    PatientId INT NOT NULL,
    Type INT NOT NULL, -- 1=WhatsApp, 2=Email, 3=SMS
    Subject NVARCHAR(200) NULL,
    Message NVARCHAR(MAX) NOT NULL,
    RecipientContact NVARCHAR(100) NOT NULL,
    Status INT NOT NULL, -- 1=Pending, 2=Sent, 3=Delivered, 4=Failed
    Reference NVARCHAR(100) NULL,
    ScheduledAt DATETIME2 NULL,
    SentAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Communications_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Communications_Patient FOREIGN KEY (PatientId) REFERENCES Patients(Id)
);

GO
