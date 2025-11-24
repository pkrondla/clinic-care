# Script to add missing columns to UserClinicAccess table

$tenantDb = "ClinicCare_demo"
$server = "localhost"
$user = "ClinicCareUser"
$password = "ClinicCare@123"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Updating UserClinicAccess Table" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$updateQuery = @"
USE $tenantDb;
GO

PRINT 'Updating UserClinicAccess table...';

-- Add IsActive column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserClinicAccess') AND name = 'IsActive')
BEGIN
    ALTER TABLE UserClinicAccess ADD IsActive BIT NOT NULL DEFAULT 1;
    PRINT 'Added IsActive column.';
END
ELSE
BEGIN
    PRINT 'IsActive column already exists.';
END

-- Add UpdatedAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserClinicAccess') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE UserClinicAccess ADD UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    PRINT 'Added UpdatedAt column.';
END
ELSE
BEGIN
    PRINT 'UpdatedAt column already exists.';
END

-- Verify the update
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'UserClinicAccess'
ORDER BY ORDINAL_POSITION;

PRINT 'UserClinicAccess table update completed.';
GO
"@

$tempFile = "temp_update_userclinicaccess.sql"
$updateQuery | Out-File -FilePath $tempFile -Encoding UTF8

Write-Host "Executing SQL update..." -ForegroundColor Yellow
$result = sqlcmd -S $server -U $user -P $password -i $tempFile -W

Write-Host ""
Write-Host "SQL Update Result:" -ForegroundColor Cyan
Write-Host $result

Remove-Item $tempFile -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "UserClinicAccess table update completed!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

