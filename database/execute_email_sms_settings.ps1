# Script to execute Email and SMS Settings migration scripts on tenant database

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
Write-Host "Email & SMS Settings Migration Scripts" -ForegroundColor Cyan
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
                Write-Host "  ✗ Error in batch: $_" -ForegroundColor Red
                $connection.Close()
                return $false
            }
        }
        
        $connection.Close()
        Write-Host "  ✓ Success" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  ✗ Exception: $_" -ForegroundColor Red
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
        return $false
    }
}

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$tenantScriptsDir = Join-Path $scriptDir "tenant"

Write-Host "Tenant Database: $DatabaseTenant" -ForegroundColor White
Write-Host ""

# Scripts to execute
$scripts = @(
    "033_CreateEmailSettings.sql",
    "034_CreateSmsSettings.sql"
)

$allSuccess = $true
foreach ($script in $scripts) {
    $scriptPath = Join-Path $tenantScriptsDir $script
    if (Test-Path $scriptPath) {
        $result = Execute-SqlScriptDotNet -ConnectionString $tenantConnectionString -ScriptPath $scriptPath -DatabaseName $DatabaseTenant
        if (-not $result) {
            $allSuccess = $false
            Write-Host "Stopping execution due to error." -ForegroundColor Red
            break
        }
    } else {
        Write-Host "  ⚠ Script not found: $scriptPath" -ForegroundColor Yellow
        $allSuccess = $false
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
if ($allSuccess) {
    Write-Host "All scripts executed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some scripts failed. Please check the errors above." -ForegroundColor Red
    exit 1
}

