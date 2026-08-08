-- ====================================
-- HomoeoDesk Global Database - Trial Requests
-- ====================================

USE HomoeoDesk_Global;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TrialRequests')
BEGIN
    CREATE TABLE TrialRequests (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(40) NULL,
        ClinicName NVARCHAR(200) NOT NULL,
        City NVARCHAR(120) NULL,
        PatientsPerWeek NVARCHAR(40) NULL,
        Message NVARCHAR(2000) NULL,
        Source NVARCHAR(100) NOT NULL DEFAULT N'website',
        Status INT NOT NULL DEFAULT 1, -- 1=New, 2=Contacted, 3=Converted, 4=Closed
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_TrialRequests_Status_CreatedAt ON TrialRequests (Status, CreatedAt DESC);
    CREATE INDEX IX_TrialRequests_Email ON TrialRequests (Email);

    PRINT 'Table TrialRequests created.';
END
ELSE
BEGIN
    PRINT 'Table TrialRequests already exists.';
END
GO
