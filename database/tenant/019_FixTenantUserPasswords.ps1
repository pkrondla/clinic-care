# Script to fix tenant user password hashes
# The password hash format in the database doesn't match what PasswordHasher expects
# This script will generate correct hashes and update the database

$tenantDb = "ClinicCare_demo"
$server = "localhost"
$user = "ClinicCareUser"
$password = "ClinicCare@123"
$passwordToHash = "Admin@123"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Fixing Tenant User Password Hashes" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Step 1: Generate password hash using API endpoint..." -ForegroundColor Yellow
Write-Host "Please call POST /api/test/generate-password-hash with body: { `"password`": `"Admin@123`" }" -ForegroundColor Yellow
Write-Host "Then update the hash below and run this script again." -ForegroundColor Yellow
Write-Host ""

# For now, we'll use a known working hash format
# The PasswordHasher creates: Base64(salt(32 bytes) + hash(32 bytes))
# This is a placeholder - you need to generate it using the API endpoint

$correctHash = "REPLACE_WITH_GENERATED_HASH"

if ($correctHash -eq "REPLACE_WITH_GENERATED_HASH") {
    Write-Host "ERROR: Please generate the hash first using the API endpoint!" -ForegroundColor Red
    Write-Host ""
    Write-Host "To generate the hash:" -ForegroundColor Yellow
    Write-Host "1. Start the API" -ForegroundColor Yellow
    Write-Host "2. Open Swagger UI: http://localhost:51537/swagger" -ForegroundColor Yellow
    Write-Host "3. Call POST /api/test/generate-password-hash" -ForegroundColor Yellow
    Write-Host "4. Use the returned hash value" -ForegroundColor Yellow
    exit 1
}

Write-Host "Step 2: Updating password hashes in database..." -ForegroundColor Yellow

$updateQuery = @"
USE $tenantDb;
GO

UPDATE Users 
SET PasswordHash = '$correctHash'
WHERE Email IN ('admin@demo.com', 'doctor@demo.com', 'reception@demo.com');
GO

SELECT Email, PasswordHash FROM Users WHERE Email IN ('admin@demo.com', 'doctor@demo.com', 'reception@demo.com');
GO
"@

$updateQuery | Out-File -FilePath "temp_update_passwords.sql" -Encoding UTF8

sqlcmd -S $server -U $user -P $password -i "temp_update_passwords.sql"

Remove-Item "temp_update_passwords.sql" -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Password hashes updated successfully!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan


