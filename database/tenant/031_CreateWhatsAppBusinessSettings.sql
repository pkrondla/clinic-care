-- ====================================
-- ClinicCare Tenant Database - Create WhatsAppBusinessSettings Table
-- ====================================
-- This script creates the WhatsAppBusinessSettings table for storing
-- organization-level WhatsApp Business API configuration
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WhatsAppBusinessSettings')
BEGIN
    CREATE TABLE WhatsAppBusinessSettings (
        Id INT PRIMARY KEY IDENTITY(1,1),
        OrganizationId INT NOT NULL,
        IsEnabled BIT NOT NULL DEFAULT 0,
        Provider INT NOT NULL DEFAULT 1, -- 1 = Meta, 2 = Twilio, 3 = Dialog360
        PhoneNumberId NVARCHAR(100) NULL,
        BusinessAccountId NVARCHAR(100) NULL,
        AccessToken NVARCHAR(1000) NULL, -- Will be encrypted
        ApiKey NVARCHAR(500) NULL, -- Will be encrypted
        ApiSecret NVARCHAR(500) NULL, -- Will be encrypted
        WebhookUrl NVARCHAR(500) NULL,
        WebhookSecret NVARCHAR(200) NULL, -- Will be encrypted
        WebhookVerifyToken NVARCHAR(200) NULL,
        ApiVersion NVARCHAR(20) NULL,
        FromPhoneNumber NVARCHAR(20) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id) ON DELETE RESTRICT
    );
    
    -- Unique constraint: One settings record per organization
    CREATE UNIQUE INDEX IX_WhatsAppBusinessSettings_OrganizationId 
        ON WhatsAppBusinessSettings(OrganizationId);
    
    -- Index for active settings
    CREATE INDEX IX_WhatsAppBusinessSettings_OrganizationId_IsEnabled_IsActive 
        ON WhatsAppBusinessSettings(OrganizationId, IsEnabled, IsActive);
    
    PRINT 'Created WhatsAppBusinessSettings table.';
END
ELSE
BEGIN
    PRINT 'WhatsAppBusinessSettings table already exists.';
END
GO

