-- ====================================
-- ClinicCare Tenant Database - Prescriptions Table
-- ====================================
-- This table stores prescription headers
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Prescriptions')
BEGIN
    CREATE TABLE Prescriptions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        ConsultationId INT NOT NULL,
        PrescriptionNumber NVARCHAR(50) NOT NULL,
        Status INT NOT NULL DEFAULT 1, -- 1=Draft, 2=Issued, 3=Dispensed
        InternalNotes NVARCHAR(MAX) NULL,
        PatientInstructions NVARCHAR(MAX) NULL,
        IssuedDate DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Prescriptions_Consultation FOREIGN KEY (ConsultationId) REFERENCES Consultations(Id),
        CONSTRAINT UK_Prescriptions_Number UNIQUE (PrescriptionNumber)
    );
    PRINT 'Table Prescriptions created.';
END
ELSE
BEGIN
    PRINT 'Table Prescriptions already exists.';
END
GO

