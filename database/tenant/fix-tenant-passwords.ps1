# Script to fix tenant user password hashes
# The password hash in database is in wrong format

$password = "Admin@123"
$apiUrl = "http://localhost:51537/api/test/generate-password-hash"

Write-Host "Generating password hash for: $password" -ForegroundColor Yellow
Write-Host "Calling API: $apiUrl" -ForegroundColor Yellow

try {
    $body = @{
        password = $password
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Body $body -ContentType "application/json"
    
    $hash = $response.hash
    Write-Host "Generated hash: $hash" -ForegroundColor Green
    
    # Update database
    $updateQuery = @"
USE ClinicCare_demo;
UPDATE Users 
SET PasswordHash = '$hash'
WHERE Email IN ('admin@demo.com', 'doctor@demo.com', 'reception@demo.com');
SELECT Email, 'Updated' AS Status FROM Users WHERE Email IN ('admin@demo.com', 'doctor@demo.com', 'reception@demo.com');
"@
    
    $updateQuery | Out-File -FilePath "temp_update.sql" -Encoding UTF8
    
    Write-Host "`nUpdating database..." -ForegroundColor Yellow
    sqlcmd -S localhost -U ClinicCareUser -P ClinicCare@123 -i temp_update.sql
    
    Remove-Item temp_update.sql -ErrorAction SilentlyContinue
    
    Write-Host "`nPassword hashes updated successfully!" -ForegroundColor Green
}
catch {
    Write-Host "`nERROR: Could not generate hash. Make sure the API is running." -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nManual steps:" -ForegroundColor Yellow
    Write-Host "1. Start the API" -ForegroundColor Yellow
    Write-Host "2. Open Swagger: http://localhost:51537/swagger" -ForegroundColor Yellow
    Write-Host "3. Call POST /api/test/generate-password-hash with: { `"password`": `"Admin@123`" }" -ForegroundColor Yellow
    Write-Host "4. Copy the hash and update the database manually" -ForegroundColor Yellow
}


