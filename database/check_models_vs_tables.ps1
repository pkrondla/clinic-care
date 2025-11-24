# Script to check all entity models against database tables
# This will identify missing columns or properties

$globalDb = "ClinicCare_Global"
$tenantDb = "ClinicCare_demo"
$server = "localhost"
$user = "ClinicCareUser"
$password = "ClinicCare@123"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Checking Entity Models vs Database Tables" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Global Database Tables
Write-Host "GLOBAL DATABASE ($globalDb):" -ForegroundColor Yellow
Write-Host "----------------------------" -ForegroundColor Yellow

$globalTables = @(
    "Organizations",
    "SystemUsers",
    "SubscriptionPlans",
    "OrganizationSubscriptions",
    "GlobalMedicines",
    "PaymentTransactions",
    "AuditLogs"
)

foreach ($table in $globalTables) {
    Write-Host "`nChecking table: $table" -ForegroundColor Green
    $query = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH 
              FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = '$table' 
              ORDER BY ORDINAL_POSITION"
    
    $result = sqlcmd -S $server -U $user -P $password -d $globalDb -Q $query -h -1 -W
    Write-Host $result
}

# Tenant Database Tables
Write-Host "`n`nTENANT DATABASE ($tenantDb):" -ForegroundColor Yellow
Write-Host "----------------------------" -ForegroundColor Yellow

$tenantTables = @(
    "Users",
    "Clinics",
    "UserClinicAccess",
    "DoctorProfiles",
    "DoctorAvailabilities",
    "Patients",
    "Appointments",
    "Consultations",
    "Prescriptions",
    "PrescriptionMedicines",
    "Medicines",
    "Inventory",
    "StockTransactions",
    "Invoices",
    "InvoiceItems",
    "Communications"
)

foreach ($table in $tenantTables) {
    Write-Host "`nChecking table: $table" -ForegroundColor Green
    $query = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH 
              FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = '$table' 
              ORDER BY ORDINAL_POSITION"
    
    $result = sqlcmd -S $server -U $user -P $password -d $tenantDb -Q $query -h -1 -W
    if ($result) {
        Write-Host $result
    } else {
        Write-Host "  Table not found!" -ForegroundColor Red
    }
}

Write-Host "`n`n=========================================" -ForegroundColor Cyan
Write-Host "Check completed!" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan


