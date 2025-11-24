-- ====================================
-- ClinicCare Tenant Database - Doctor Availabilities Table
-- ====================================
-- This table stores doctor availability schedules
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DoctorAvailabilities')
BEGIN
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
        
        CONSTRAINT FK_DoctorAvailabilities_Doctor FOREIGN KEY (DoctorId) REFERENCES Users(Id),
        CONSTRAINT FK_DoctorAvailabilities_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id)
    );
    PRINT 'Table DoctorAvailabilities created.';
END
ELSE
BEGIN
    PRINT 'Table DoctorAvailabilities already exists.';
END
GO

