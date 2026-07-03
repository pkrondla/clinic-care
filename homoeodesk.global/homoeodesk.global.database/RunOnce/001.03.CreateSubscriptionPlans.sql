-- ====================================
-- HomoeoDesk Global Database - Subscription Plans Table
-- ====================================
-- This table stores available subscription plans
-- ====================================

USE HomoeoDesk_Global;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubscriptionPlans')
BEGIN
    CREATE TABLE SubscriptionPlans (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        Price DECIMAL(10,2) NOT NULL,
        BillingCycle INT NOT NULL, -- 1=Monthly, 2=Quarterly, 3=Yearly
        MaxClinics INT NOT NULL,
        MaxDoctors INT NOT NULL,
        MaxPatients INT NOT NULL,
        Features NVARCHAR(MAX) NULL, -- JSON array of features
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table SubscriptionPlans created.';
END
ELSE
BEGIN
    PRINT 'Table SubscriptionPlans already exists.';
END
GO


