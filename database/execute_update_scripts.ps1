# Script to execute all UPDATE database scripts for Tenant database
# These scripts modify existing tables to add new columns or update schema

param(
    [string]$Server = "localhost",
    [string]$DatabaseTenant = "ClinicCare_demo",
    [string]$UserId = "ClinicCareUser",
    [string]$Password = "ClinicCare@123"
)

$ErrorActionPreference = "Stop"

# Build connection string
$tenantConnectionString = "Server=$Server;Database=$DatabaseTenant;User Id=$UserId;Password=$Password;TrustServerCertificate=True;MultipleActiveResultSets=true"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Database Update Script Execution" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Function to execute SQL script using .NET
function Execute-SqlScriptDotNet {
    param(
        [string]$ConnectionString,
        [string]$ScriptPath,
        [string]$DatabaseName
    )
    
    try {
        Write-Host "Executing: $ScriptPath" -ForegroundColor Yellow
        
        # Load System.Data.SqlClient
        Add-Type -AssemblyName System.Data
        
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        
        $scriptContent = Get-Content -Path $ScriptPath -Raw -Encoding UTF8
        
        # Replace {TENANT_ID} placeholder with actual tenant database name
        $scriptContent = $scriptContent -replace '\{TENANT_ID\}', $DatabaseName.Replace('ClinicCare_', '')
        $scriptContent = $scriptContent -replace 'ClinicCare_\{TENANT_ID\}', $DatabaseName
        
        # Split script by GO statements
        $batches = $scriptContent -split '(?m)^\s*GO\s*$'
        
        foreach ($batch in $batches) {
            $batch = $batch.Trim()
            if ([string]::IsNullOrWhiteSpace($batch)) {
                continue
            }
            
            $command = $connection.CreateCommand()
            $command.CommandText = $batch
            $command.CommandTimeout = 300 # 5 minutes timeout
            
            try {
                $command.ExecuteNonQuery() | Out-Null
            }
            catch {
                # Check if error is about column/object already existing (which is OK for updates)
                if ($_.Exception.Message -match "already exists" -or 
                    $_.Exception.Message -match "duplicate column name" -or
                    $_.Exception.Message -match "There is already an object") {
                    Write-Host "  ⚠ Already exists (skipping): $($_.Exception.Message)" -ForegroundColor Yellow
                } else {
                    Write-Host "  ✗ Error in batch: $_" -ForegroundColor Red
                    $connection.Close()
                    return $false
                }
            }
        }
        
        $connection.Close()
        Write-Host "  ✓ Success" -ForegroundColor Green
        return $true
    }
    catch {
        # Check if error is about column/object already existing (which is OK for updates)
        if ($_.Exception.Message -match "already exists" -or 
            $_.Exception.Message -match "duplicate column name" -or
            $_.Exception.Message -match "There is already an object") {
            Write-Host "  ⚠ Already exists (skipping): $($_.Exception.Message)" -ForegroundColor Yellow
            if ($connection.State -eq 'Open') {
                $connection.Close()
            }
            return $true
        } else {
            Write-Host "  ✗ Exception: $_" -ForegroundColor Red
            if ($connection.State -eq 'Open') {
                $connection.Close()
            }
            return $false
        }
    }
}

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$tenantScriptsDir = Join-Path $scriptDir "tenant"

Write-Host "Tenant Database Update Scripts" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Database: $DatabaseTenant" -ForegroundColor White
Write-Host ""

# Execute tenant update scripts in order
$updateScripts = @(
    "017_UpdateUsersTable.sql",
    "018_UpdateDoctorProfilesTable.sql",
    "019_UpdateClinicsTable.sql",
    "020_UpdateUserClinicAccessTable.sql",
    "021_UpdatePatientsTable.sql",
    "022_UpdateAppointmentsTable.sql"
)

$success = $true
foreach ($script in $updateScripts) {
    $scriptPath = Join-Path $tenantScriptsDir $script
    if (Test-Path $scriptPath) {
        $result = Execute-SqlScriptDotNet -ConnectionString $tenantConnectionString -ScriptPath $scriptPath -DatabaseName $DatabaseTenant
        if (-not $result) {
            $success = $false
            Write-Host "Stopping execution due to error." -ForegroundColor Red
            break
        }
    } else {
        Write-Host "  ⚠ Script not found: $scriptPath" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
if ($success) {
    Write-Host "All update scripts executed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some update scripts failed. Please check the errors above." -ForegroundColor Red
    exit 1
}

