-- ====================================
-- ClinicCare Tenant Database - Communications Table
-- ====================================
-- This table stores communication records (SMS, Email, WhatsApp)
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Communications')
BEGIN
    CREATE TABLE Communications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        PatientId INT NOT NULL,
        Type INT NOT NULL, -- 1=SMS, 2=Email, 3=WhatsApp
        Subject NVARCHAR(200) NULL,
        Message NVARCHAR(MAX) NOT NULL,
        RecipientContact NVARCHAR(100) NOT NULL,
        Status INT NOT NULL, -- 1=Pending, 2=Sent, 3=Failed
        Reference NVARCHAR(100) NULL,
        ScheduledAt DATETIME2 NULL,
        SentAt DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Communications_Patient FOREIGN KEY (PatientId) REFERENCES Patients(Id)
    );
    PRINT 'Table Communications created.';
END
ELSE
BEGIN
    PRINT 'Table Communications already exists.';
END
GO

