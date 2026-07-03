-- Add base clinic mapping to DoctorProfiles table

PRINT 'Adding base clinic mapping to DoctorProfiles table...';

-- Add BaseClinicId column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DoctorProfiles') AND name = 'BaseClinicId')
BEGIN
    ALTER TABLE dbo.DoctorProfiles
    ADD BaseClinicId INT NULL;
    PRINT 'Added BaseClinicId column.';
    
    -- Add foreign key constraint
    ALTER TABLE dbo.DoctorProfiles
    ADD CONSTRAINT FK_DoctorProfiles_BaseClinic 
    FOREIGN KEY (BaseClinicId) REFERENCES dbo.Clinics(Id)
    ON DELETE NO ACTION;
    PRINT 'Added FK_DoctorProfiles_BaseClinic constraint.';
END

PRINT 'Doctor base clinic mapping added successfully.';
GO

-- Set base clinic for existing doctors (set to first available clinic in UserClinicAccess)
UPDATE dp
SET dp.BaseClinicId = (
    SELECT TOP 1 uca.ClinicId 
    FROM dbo.UserClinicAccess uca
    INNER JOIN dbo.Users u ON uca.UserId = u.Id
    WHERE u.Id = dp.UserId
    ORDER BY uca.Id
)
FROM dbo.DoctorProfiles dp
WHERE dp.BaseClinicId IS NULL;

PRINT 'Updated existing doctors with base clinic.';
GO


