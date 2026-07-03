-- ====================================
-- HomoeoDesk Global Database - Organization Subscriptions Table
-- ====================================
-- This table tracks active subscriptions per organization
-- ====================================

USE HomoeoDesk_Global;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrganizationSubscriptions')
BEGIN
    CREATE TABLE OrganizationSubscriptions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        SubscriptionPlanId INT NOT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NOT NULL,
        Status INT NOT NULL DEFAULT 1, -- 1=Active, 2=Expired, 3=Cancelled
        AutoRenew BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_OrgSubscriptions_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
        CONSTRAINT FK_OrgSubscriptions_Plan FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id)
    );
    PRINT 'Table OrganizationSubscriptions created.';
END
ELSE
BEGIN
    PRINT 'Table OrganizationSubscriptions already exists.';
END
GO


