-- ====================================
-- ClinicCare Tenant Database - Consultations Table
-- ====================================
-- This table stores medical consultation records
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Consultations')
BEGIN
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
        
        CONSTRAINT FK_Consultations_Appointment FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
        CONSTRAINT FK_Consultations_Doctor FOREIGN KEY (DoctorId) REFERENCES Users(Id),
        CONSTRAINT FK_Consultations_Patient FOREIGN KEY (PatientId) REFERENCES Patients(Id)
    );
    PRINT 'Table Consultations created.';
END
ELSE
BEGIN
    PRINT 'Table Consultations already exists.';
END
GO

