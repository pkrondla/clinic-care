# Deploy HomoeoDesk database scripts from SSDT project folders
param(
    [string]$Server = "localhost",
    [string]$DatabaseGlobal = "HomoeoDesk_Global",
    [string]$DatabaseTenant = "HomoeoDesk_demo",
    [string]$UserId = "HomoeoDeskUser",
    [string]$Password = "HomoeoDesk@123",
    [int]$TenantId = 1,
    [switch]$GlobalOnly,
    [switch]$TenantOnly
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

function Ensure-Database {
    param([string]$Name)
    $master = "Server=$Server;Database=master;User Id=$UserId;Password=$Password;TrustServerCertificate=True"
    Add-Type -AssemblyName System.Data
    $conn = New-Object System.Data.SqlClient.SqlConnection($master)
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "IF DB_ID(N'$Name') IS NULL CREATE DATABASE [$Name];"
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()
}

function Invoke-SqlFile {
    param([string]$DatabaseName, [string]$FilePath)
    Write-Host "  $([IO.Path]::GetFileName($FilePath))" -ForegroundColor Yellow
    $content = Get-Content $FilePath -Raw -Encoding UTF8
    $content = $content -replace '\$\(TenantId\)', $TenantId
    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        $content | sqlcmd -S $Server -d $DatabaseName -U $UserId -P $Password -C -b
        if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed for $FilePath" }
    } else {
        $cs = "Server=$Server;Database=$DatabaseName;User Id=$UserId;Password=$Password;TrustServerCertificate=True"
        Add-Type -AssemblyName System.Data
        $conn = New-Object System.Data.SqlClient.SqlConnection($cs)
        $conn.Open()
        foreach ($batch in ($content -split '\r?\nGO\r?\n')) {
            if ([string]::IsNullOrWhiteSpace($batch)) { continue }
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $batch
            $cmd.CommandTimeout = 120
            $cmd.ExecuteNonQuery() | Out-Null
        }
        $conn.Close()
    }
    Write-Host "    OK" -ForegroundColor Green
}

function Deploy-Project {
    param([string]$ProjectPath, [string]$DatabaseName)
    Ensure-Database $DatabaseName
    Write-Host "`nDeploying to $DatabaseName" -ForegroundColor Cyan
    Get-ChildItem "$ProjectPath\RunOnce\*.sql" | Sort-Object Name | ForEach-Object {
        Invoke-SqlFile -DatabaseName $DatabaseName -FilePath $_.FullName
    }
    if (Test-Path "$ProjectPath\Seeds") {
        Get-ChildItem "$ProjectPath\Seeds\*.sql" | Sort-Object Name | ForEach-Object {
            Invoke-SqlFile -DatabaseName $DatabaseName -FilePath $_.FullName
        }
    }
    if (Test-Path "$ProjectPath\Versioned") {
        Get-ChildItem "$ProjectPath\Versioned\*.sql" | Sort-Object Name | ForEach-Object {
            Invoke-SqlFile -DatabaseName $DatabaseName -FilePath $_.FullName
        }
    }
    $postDeploy = Join-Path $ProjectPath "PostDeploy.sql"
    if (Test-Path $postDeploy) {
        Write-Host "  PostDeploy.sql" -ForegroundColor Yellow
        Invoke-SqlFile -DatabaseName $DatabaseName -FilePath $postDeploy
    }
}

Write-Host "HomoeoDesk Database Deploy" -ForegroundColor Cyan
Write-Host "Server: $Server"

if (-not $TenantOnly) {
    Deploy-Project "$root\homoeodesk.global\homoeodesk.global.database" $DatabaseGlobal
}
if (-not $GlobalOnly) {
    Deploy-Project "$root\homoeodesk.tenant\homoeodesk.tenant.database" $DatabaseTenant
}

Write-Host "`nDone." -ForegroundColor Green
