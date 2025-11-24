# Script to update all user passwords in tenant database
# Calls the API to generate password hash, then updates all users

$apiUrl = "http://localhost:51537/api/test/generate-password-hash"
$password = "Admin@123"
$tenantDb = "ClinicCare_demo"
$server = "localhost"
$dbUser = "ClinicCareUser"
$dbPassword = "ClinicCare@123"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Updating All Tenant User Passwords" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Step 1: Generating password hash for '$password'..." -ForegroundColor Yellow

try {
    $body = @{
        password = $password
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Body $body -ContentType "application/json"
    $hash = $response.hash
    
    Write-Host "Generated hash: $hash" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Step 2: Updating all user passwords in database..." -ForegroundColor Yellow
    
    # Create SQL update script
    $updateQuery = @"
USE $tenantDb;
GO

-- Update all users with the new password hash
UPDATE Users 
SET PasswordHash = '$hash',
    UpdatedAt = GETUTCDATE()
WHERE IsActive = 1;
GO

-- Show updated users
SELECT 
    Id,
    Email,
    FirstName,
    LastName,
    Role,
    CASE 
        WHEN PasswordHash LIKE '1000:%' THEN 'OLD_FORMAT'
        ELSE 'NEW_FORMAT'
    END AS HashFormat,
    LEN(PasswordHash) AS HashLength,
    UpdatedAt
FROM Users
WHERE IsActive = 1
ORDER BY Email;
GO
"@
    
    # Write to temp file
    $tempFile = "temp_update_passwords.sql"
    $updateQuery | Out-File -FilePath $tempFile -Encoding UTF8
    
    # Execute SQL
    Write-Host "Executing SQL update..." -ForegroundColor Yellow
    $sqlResult = sqlcmd -S $server -U $dbUser -P $dbPassword -i $tempFile -W
    
    Write-Host ""
    Write-Host "SQL Update Result:" -ForegroundColor Cyan
    Write-Host $sqlResult
    
    # Clean up
    Remove-Item $tempFile -ErrorAction SilentlyContinue
    
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "Password update completed successfully!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "All active users now have password: $password" -ForegroundColor Yellow
}
catch {
    Write-Host ""
    Write-Host "ERROR: Failed to update passwords" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure:" -ForegroundColor Yellow
    Write-Host "1. The API is running on http://localhost:51537" -ForegroundColor Yellow
    Write-Host "2. SQL Server is accessible" -ForegroundColor Yellow
    Write-Host "3. Database credentials are correct" -ForegroundColor Yellow
    exit 1
}

