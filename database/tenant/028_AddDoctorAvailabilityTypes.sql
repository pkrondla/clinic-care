-- Add availability type and notes to DoctorAvailabilities table

PRINT 'Adding availability type to DoctorAvailabilities table...';

-- Add AvailabilityType column (0 = Regular, 1 = DifferentClinic, 2 = Leave, 3 = ModifiedHours)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DoctorAvailabilities') AND name = 'AvailabilityType')
BEGIN
    ALTER TABLE dbo.DoctorAvailabilities
    ADD AvailabilityType INT NOT NULL DEFAULT 0;
    PRINT 'Added AvailabilityType column.';
END

-- Add Notes column for additional information
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DoctorAvailabilities') AND name = 'Notes')
BEGIN
    ALTER TABLE dbo.DoctorAvailabilities
    ADD Notes NVARCHAR(500) NULL;
    PRINT 'Added Notes column.';
END

-- Add EndDate for leave date ranges
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DoctorAvailabilities') AND name = 'EndDate')
BEGIN
    ALTER TABLE dbo.DoctorAvailabilities
    ADD EndDate DATE NULL;
    PRINT 'Added EndDate column.';
END

PRINT 'Doctor availability types added successfully.';
GO

