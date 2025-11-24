# Script to add OrganizationId column to Clinics table

$tenantDb = "ClinicCare_demo"
$server = "localhost"
$user = "ClinicCareUser"
$password = "ClinicCare@123"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Updating Clinics Table" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$updateQuery = @"
USE $tenantDb;
GO

PRINT 'Updating Clinics table...';

-- Add OrganizationId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clinics') AND name = 'OrganizationId')
BEGIN
    -- Add column with default value (this will automatically set all existing rows to 1)
    ALTER TABLE Clinics ADD OrganizationId INT NOT NULL DEFAULT 1;
    PRINT 'Added OrganizationId column with default value 1.';
    PRINT 'All existing clinic records have been set to OrganizationId = 1.';
END
ELSE
BEGIN
    PRINT 'OrganizationId column already exists.';
END

-- Verify the update
SELECT 
    c.Id,
    c.Name,
    c.Code,
    c.OrganizationId,
    c.IsActive
FROM Clinics c
ORDER BY c.Id;

PRINT 'Clinics table update completed.';
GO
"@

$tempFile = "temp_update_clinics.sql"
$updateQuery | Out-File -FilePath $tempFile -Encoding UTF8

Write-Host "Executing SQL update..." -ForegroundColor Yellow
$result = sqlcmd -S $server -U $user -P $password -i $tempFile -W

Write-Host ""
Write-Host "SQL Update Result:" -ForegroundColor Cyan
Write-Host $result

Remove-Item $tempFile -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Clinics table update completed!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

