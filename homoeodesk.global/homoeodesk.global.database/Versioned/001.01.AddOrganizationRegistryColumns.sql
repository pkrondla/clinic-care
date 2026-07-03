-- Add columns introduced after initial ClinicCare global schema
USE HomoeoDesk_Global;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Organizations') AND name = N'DatabaseName')
BEGIN
    ALTER TABLE dbo.Organizations ADD DatabaseName NVARCHAR(100) NOT NULL CONSTRAINT DF_Organizations_DatabaseName DEFAULT '';
    PRINT 'Added Organizations.DatabaseName';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Organizations') AND name = N'SubscriptionStatus')
BEGIN
    ALTER TABLE dbo.Organizations ADD SubscriptionStatus INT NOT NULL CONSTRAINT DF_Organizations_SubscriptionStatus DEFAULT 1;
    PRINT 'Added Organizations.SubscriptionStatus';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Organizations') AND name = N'TrialEndDate')
BEGIN
    ALTER TABLE dbo.Organizations ADD TrialEndDate DATETIME2 NULL;
    PRINT 'Added Organizations.TrialEndDate';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Organizations_DatabaseName' AND object_id = OBJECT_ID(N'dbo.Organizations'))
BEGIN
    CREATE UNIQUE INDEX IX_Organizations_DatabaseName ON dbo.Organizations(DatabaseName) WHERE DatabaseName <> '';
    PRINT 'Added IX_Organizations_DatabaseName';
END
GO
