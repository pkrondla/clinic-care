-- ====================================
-- HomoeoDesk Tenant Database - Invoices Table
-- ====================================
-- This table stores billing and payment records
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
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
        Status INT NOT NULL, -- 1=Pending, 2=Paid, 3=PartiallyPaid, 4=Refunded
        PaymentMethod NVARCHAR(50) NULL,
        PaymentReference NVARCHAR(100) NULL,
        InvoiceDate DATETIME2 NOT NULL,
        PaymentDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Invoices_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
        CONSTRAINT FK_Invoices_Patient FOREIGN KEY (PatientId) REFERENCES Patients(Id),
        CONSTRAINT FK_Invoices_Consultation FOREIGN KEY (ConsultationId) REFERENCES Consultations(Id),
        CONSTRAINT UK_Invoices_Number UNIQUE (InvoiceNumber)
    );
    PRINT 'Table Invoices created.';
END
ELSE
BEGIN
    PRINT 'Table Invoices already exists.';
END
GO


