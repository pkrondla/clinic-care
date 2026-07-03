# Validate and optionally build DACPAC from SSDT projects.
# Note: Microsoft.Build.Sql requires NuGet.Build.Tasks.Pack (not available on all SDK installs).
# Local publish: use scripts/deploy-databases.ps1 (RunOnce → Seeds → Versioned → PostDeploy).
param(
    [ValidateSet("global", "tenant", "all")]
    [string]$Project = "all",
    [string]$Configuration = "Release",
    [switch]$TryDacpac
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

function Validate-Scripts {
    param([string]$ProjectPath, [string]$Name)
    Write-Host "Validating SQL scripts: $Name" -ForegroundColor Cyan
    Push-Location $ProjectPath
    try {
        dotnet build "$Name.sqlproj" -c $Configuration
        $count = (Get-ChildItem -Recurse -Filter "*.sql" | Measure-Object).Count
        Write-Host "  $count SQL files validated" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

function Try-BuildDacpac {
    param([string]$ProjectPath, [string]$Name)
    Write-Host "Attempting DACPAC build: $Name" -ForegroundColor Cyan
    Push-Location $ProjectPath
    try {
        $sdkProj = @"
<Project Sdk="Microsoft.Build.Sql/1.0.0">
  <PropertyGroup><Name>$Name</Name></PropertyGroup>
  <ItemGroup>
    <None Include="RunOnce\**\*.sql" />
    <None Include="Seeds\**\*.sql" />
    <None Include="Versioned\**\*.sql" />
    <PostDeploy Include="PostDeploy.sql" />
  </ItemGroup>
</Project>
"@
        $tempProj = Join-Path $env:TEMP "$Name.dacpac.sqlproj"
        Set-Content -Path $tempProj -Value $sdkProj -Encoding UTF8
        dotnet build $tempProj -c $Configuration 2>&1 | Out-Null
        $dacpac = Get-ChildItem -Recurse -Filter "*.dacpac" -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($dacpac) {
            Write-Host "  DACPAC: $($dacpac.FullName)" -ForegroundColor Green
        } else {
            Write-Host "  DACPAC build unavailable — use deploy-databases.ps1 for script publish" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "  DACPAC skipped: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    finally {
        Pop-Location
    }
}

if ($Project -in @("global", "all")) {
    Validate-Scripts "$root\homoeodesk.global\homoeodesk.global.database" "homoeodesk.global.database"
    if ($TryDacpac) { Try-BuildDacpac "$root\homoeodesk.global\homoeodesk.global.database" "homoeodesk.global.database" }
}
if ($Project -in @("tenant", "all")) {
    Validate-Scripts "$root\homoeodesk.tenant\homoeodesk.tenant.database" "homoeodesk.tenant.database"
    if ($TryDacpac) { Try-BuildDacpac "$root\homoeodesk.tenant\homoeodesk.tenant.database" "homoeodesk.tenant.database" }
}

Write-Host "`nPublish locally: .\scripts\deploy-databases.ps1" -ForegroundColor Cyan
