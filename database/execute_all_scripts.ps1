# Script to execute all database scripts for Global and Tenant databases
# This script will create all tables in the correct order

param(
    [string]$Server = "localhost",
    [string]$DatabaseGlobal = "ClinicCare_Global",
    [string]$DatabaseTenant = "ClinicCare_demo",
    [string]$UserId = "ClinicCareUser",
    [string]$Password = "ClinicCare@123"
)

$ErrorActionPreference = "Stop"

# Build connection strings
$globalConnectionString = "Server=$Server;Database=$DatabaseGlobal;User Id=$UserId;Password=$Password;TrustServerCertificate=True;MultipleActiveResultSets=true"
$tenantConnectionString = "Server=$Server;Database=$DatabaseTenant;User Id=$UserId;Password=$Password;TrustServerCertificate=True;MultipleActiveResultSets=true"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Database Script Execution" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Function to execute SQL script
function Execute-SqlScript {
    param(
        [string]$ConnectionString,
        [string]$ScriptPath,
        [string]$DatabaseName
    )
    
    try {
        Write-Host "Executing: $ScriptPath" -ForegroundColor Yellow
        $scriptContent = Get-Content -Path $ScriptPath -Raw -Encoding UTF8
        
        # Execute using sqlcmd
        $result = sqlcmd -S $Server -d $DatabaseName -U $UserId -P $Password -C -Q $scriptContent 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Success" -ForegroundColor Green
            return $true
        } else {
            Write-Host "  ✗ Failed" -ForegroundColor Red
            Write-Host "  Error: $result" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "  ✗ Exception: $_" -ForegroundColor Red
        return $false
    }
}

# Check if sqlcmd is available
try {
    $null = Get-Command sqlcmd -ErrorAction Stop
    Write-Host "sqlcmd found. Using sqlcmd to execute scripts." -ForegroundColor Green
    $useSqlCmd = $true
}
catch {
    Write-Host "sqlcmd not found. Attempting to use .NET SqlConnection." -ForegroundColor Yellow
    $useSqlCmd = $false
}

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
$globalScriptsDir = Join-Path $scriptDir "global"
$tenantScriptsDir = Join-Path $scriptDir "tenant"

Write-Host "Global Database Scripts" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan
Write-Host "Database: $DatabaseGlobal" -ForegroundColor White
Write-Host ""

# Execute global scripts in order
$globalScripts = @(
    "001_CreateOrganizations.sql",
    "002_CreateGlobalMedicines.sql",
    "003_CreateSubscriptionPlans.sql",
    "004_CreateOrganizationSubscriptions.sql",
    "005_CreateSystemUsers.sql",
    "006_CreateAuditLogs.sql",
    "007_CreatePaymentTransactions.sql"
)

$globalSuccess = $true
foreach ($script in $globalScripts) {
    $scriptPath = Join-Path $globalScriptsDir $script
    if (Test-Path $scriptPath) {
        if ($useSqlCmd) {
            $result = Execute-SqlScript -ConnectionString $globalConnectionString -ScriptPath $scriptPath -DatabaseName $DatabaseGlobal
        } else {
            $result = Execute-SqlScriptDotNet -ConnectionString $globalConnectionString -ScriptPath $scriptPath -DatabaseName $DatabaseGlobal
        }
        if (-not $result) {
            $globalSuccess = $false
            Write-Host "Stopping execution due to error." -ForegroundColor Red
            break
        }
    } else {
        Write-Host "  ⚠ Script not found: $scriptPath" -ForegroundColor Yellow
    }
}

if (-not $globalSuccess) {
    Write-Host ""
    Write-Host "Global database scripts failed. Aborting tenant scripts." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Tenant Database Scripts" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan
Write-Host "Database: $DatabaseTenant" -ForegroundColor White
Write-Host ""

# Execute tenant scripts in order (excluding update scripts for now)
$tenantScripts = @(
    "001_CreateUsers.sql",
    "002_CreateUserOrganizations.sql",
    "003_CreateClinics.sql",
    "004_CreatePatients.sql",
    "005_CreateDoctorProfiles.sql",
    "006_CreateDoctorAvailabilities.sql",
    "007_CreateAppointments.sql",
    "008_CreateConsultations.sql",
    "009_CreateClinicMedicines.sql",
    "010_CreatePrescriptions.sql",
    "011_CreatePrescriptionItems.sql",
    "012_CreateInventories.sql",
    "013_CreateStockTransactions.sql",
    "014_CreateInvoices.sql",
    "015_CreateInvoiceItems.sql",
    "016_CreateCommunications.sql",
    "018_CreateSuppliers.sql",
    "019_CreatePurchaseOrders.sql"
)

$tenantSuccess = $true
foreach ($script in $tenantScripts) {
    $scriptPath = Join-Path $tenantScriptsDir $script
    if (Test-Path $scriptPath) {
        if ($useSqlCmd) {
            $result = Execute-SqlScript -ConnectionString $tenantConnectionString -ScriptPath $scriptPath -DatabaseName $DatabaseTenant
        } else {
            $result = Execute-SqlScriptDotNet -ConnectionString $tenantConnectionString -ScriptPath $scriptPath -DatabaseName $DatabaseTenant
        }
        if (-not $result) {
            $tenantSuccess = $false
            Write-Host "Stopping execution due to error." -ForegroundColor Red
            break
        }
    } else {
        Write-Host "  ⚠ Script not found: $scriptPath" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
if ($globalSuccess -and $tenantSuccess) {
    Write-Host "All scripts executed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some scripts failed. Please check the errors above." -ForegroundColor Red
    exit 1
}

