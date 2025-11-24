-- ====================================
-- ClinicCare Tenant Database - Patients Table
-- ====================================
-- This table stores patient records
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Patients')
BEGIN
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
        
        CONSTRAINT UK_Patients_PatientCode UNIQUE (PatientCode),
        CONSTRAINT FK_Patients_User FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT 'Table Patients created.';
END
ELSE
BEGIN
    PRINT 'Table Patients already exists.';
END
GO

