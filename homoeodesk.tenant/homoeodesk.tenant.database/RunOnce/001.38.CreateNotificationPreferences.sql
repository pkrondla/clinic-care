-- ====================================
-- HomoeoDesk Tenant Database - Create NotificationPreferences Table
-- ====================================
-- This script creates the NotificationPreferences table for storing
-- organization-level notification preferences per notification type
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationPreferences')
BEGIN
    CREATE TABLE NotificationPreferences (
        Id INT PRIMARY KEY IDENTITY(1,1),
        OrganizationId INT NOT NULL,
        NotificationType INT NOT NULL, -- Enum: AppointmentCreated, PrescriptionCreated, etc.
        EnableWhatsApp BIT NOT NULL DEFAULT 1,
        EnableEmail BIT NOT NULL DEFAULT 1,
        EnableSMS BIT NOT NULL DEFAULT 0,
        Template NVARCHAR(MAX) NULL, -- Custom message template (optional)
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id) ON DELETE RESTRICT
    );
    
    -- Unique constraint: One preference per organization per notification type
    CREATE UNIQUE INDEX IX_NotificationPreferences_OrganizationId_NotificationType 
        ON NotificationPreferences(OrganizationId, NotificationType);
    
    -- Index for active preferences
    CREATE INDEX IX_NotificationPreferences_OrganizationId_IsActive 
        ON NotificationPreferences(OrganizationId, IsActive);
    
    PRINT 'Created NotificationPreferences table.';
END
ELSE
BEGIN
    PRINT 'NotificationPreferences table already exists.';
END
GO


