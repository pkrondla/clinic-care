-- ====================================
-- HomoeoDesk Tenant Database - Doctor Profiles Table
-- ====================================
-- This table stores doctor-specific information
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DoctorProfiles')
BEGIN
    CREATE TABLE DoctorProfiles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        UserId INT NOT NULL,
        RegistrationNumber NVARCHAR(100) NOT NULL,
        Qualification NVARCHAR(500) NOT NULL,
        ExperienceYears INT NOT NULL DEFAULT 0,
        Specialization NVARCHAR(200) NULL,
        ConsultationFeeInPerson DECIMAL(10,2) NOT NULL DEFAULT 0,
        ConsultationFeeTele DECIMAL(10,2) NOT NULL DEFAULT 0,
        FollowupFeeInPerson DECIMAL(10,2) NOT NULL DEFAULT 0,
        FollowupFeeTele DECIMAL(10,2) NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_DoctorProfiles_User FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT UK_DoctorProfiles_UserId UNIQUE (UserId)
    );
    PRINT 'Table DoctorProfiles created.';
END
ELSE
BEGIN
    PRINT 'Table DoctorProfiles already exists.';
END
GO


