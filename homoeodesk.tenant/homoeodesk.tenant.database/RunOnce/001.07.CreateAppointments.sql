-- ====================================
-- HomoeoDesk Tenant Database - Appointments Table
-- ====================================
-- This table stores appointment bookings and tokens
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Appointments')
BEGIN
    CREATE TABLE Appointments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        ClinicId INT NOT NULL,
        DoctorId INT NOT NULL,
        PatientId INT NOT NULL,
        AppointmentDate DATE NOT NULL,
        TokenNumber INT NOT NULL,
        Type INT NOT NULL, -- 1=InPerson, 2=Teleconsultation
        Status INT NOT NULL DEFAULT 1, -- 1=Scheduled, 2=InProgress, 3=Completed, 4=Cancelled
        Notes NVARCHAR(1000) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Appointments_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
        CONSTRAINT FK_Appointments_Doctor FOREIGN KEY (DoctorId) REFERENCES Users(Id),
        CONSTRAINT FK_Appointments_Patient FOREIGN KEY (PatientId) REFERENCES Patients(Id)
    );
    PRINT 'Table Appointments created.';
END
ELSE
BEGIN
    PRINT 'Table Appointments already exists.';
END
GO


