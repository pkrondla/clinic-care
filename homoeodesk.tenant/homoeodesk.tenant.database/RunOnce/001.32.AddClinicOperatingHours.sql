-- Add operating hours to Clinics table

PRINT 'Adding operating hours columns to Clinics table...';

-- Add operating hours type (0 = SingleShift, 1 = SplitShift)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clinics') AND name = 'OperatingHoursType')
BEGIN
    ALTER TABLE dbo.Clinics
    ADD OperatingHoursType INT NOT NULL DEFAULT 0;
    PRINT 'Added OperatingHoursType column.';
END

-- Add morning shift timings
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clinics') AND name = 'MorningStartTime')
BEGIN
    ALTER TABLE dbo.Clinics
    ADD MorningStartTime TIME NULL;
    PRINT 'Added MorningStartTime column.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clinics') AND name = 'MorningEndTime')
BEGIN
    ALTER TABLE dbo.Clinics
    ADD MorningEndTime TIME NULL;
    PRINT 'Added MorningEndTime column.';
END

-- Add evening shift timings
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clinics') AND name = 'EveningStartTime')
BEGIN
    ALTER TABLE dbo.Clinics
    ADD EveningStartTime TIME NULL;
    PRINT 'Added EveningStartTime column.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clinics') AND name = 'EveningEndTime')
BEGIN
    ALTER TABLE dbo.Clinics
    ADD EveningEndTime TIME NULL;
    PRINT 'Added EveningEndTime column.';
END

-- Add full day shift timings
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clinics') AND name = 'FullDayStartTime')
BEGIN
    ALTER TABLE dbo.Clinics
    ADD FullDayStartTime TIME NULL;
    PRINT 'Added FullDayStartTime column.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clinics') AND name = 'FullDayEndTime')
BEGIN
    ALTER TABLE dbo.Clinics
    ADD FullDayEndTime TIME NULL;
    PRINT 'Added FullDayEndTime column.';
END

PRINT 'Clinic operating hours schema updated successfully.';
GO

-- Update existing clinics with default timings (SingleShift: 10 AM - 5 PM)
UPDATE dbo.Clinics
SET 
    OperatingHoursType = 0,
    FullDayStartTime = '10:00:00',
    FullDayEndTime = '17:00:00'
WHERE FullDayStartTime IS NULL;

PRINT 'Updated existing clinics with default timings.';
GO


