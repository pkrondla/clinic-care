-- ====================================
-- HomoeoDesk Tenant Database - User Organizations Table
-- ====================================
-- This table maps users to organizations (for multi-tenant user access)
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserOrganizations')
BEGIN
    CREATE TABLE UserOrganizations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        OrganizationId INT NOT NULL,
        Role INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_UserOrganizations_User FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT UK_UserOrganizations_User_Org UNIQUE (UserId, OrganizationId)
    );
    PRINT 'Table UserOrganizations created.';
END
ELSE
BEGIN
    PRINT 'Table UserOrganizations already exists.';
END
GO


