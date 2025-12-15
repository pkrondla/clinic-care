-- ====================================
-- ClinicCare Tenant Database - ConsultationPhotos Table
-- ====================================
-- This table stores photos associated with consultations
-- Photos are stored as base64-encoded strings in the PhotoUrl column
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE ClinicCare_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConsultationPhotos')
BEGIN
    CREATE TABLE ConsultationPhotos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ConsultationId INT NOT NULL,
        PhotoUrl NVARCHAR(MAX) NOT NULL,
        Description NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        OrganizationId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_ConsultationPhotos_Consultations FOREIGN KEY (ConsultationId)
            REFERENCES Consultations(Id) ON DELETE CASCADE
    );
    
    -- Create index for ConsultationId for faster lookups
    CREATE NONCLUSTERED INDEX IX_ConsultationPhotos_ConsultationId
        ON ConsultationPhotos (ConsultationId);
    
    PRINT 'Table ConsultationPhotos created.';
END
ELSE
BEGIN
    PRINT 'Table ConsultationPhotos already exists.';
END
GO

