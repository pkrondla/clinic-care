param(
    [Parameter(Mandatory = $true)][string]$ResourceGroup,
    [Parameter(Mandatory = $true)][string]$AppName,
    [Parameter(Mandatory = $true)][int]$TenantId,
    [string]$ConnectionString = "",
    [string[]]$CorsOrigins = @()
)

$ErrorActionPreference = "Stop"

Write-Host "Configuring stamp app: $AppName (TenantId=$TenantId)"

$settings = @{
    "TenantStamp__EnableFixedTenant" = "true"
    "TenantStamp__FixedTenantId" = "$TenantId"
    "SeedOnStartup" = "false"
}

if ($ConnectionString) {
    $settings["TenantStamp__FixedTenantConnectionString"] = $ConnectionString
}

if ($CorsOrigins.Count -gt 0) {
    $settings["Cors__AllowedOrigins__0"] = $CorsOrigins[0]
    for ($i = 1; $i -lt $CorsOrigins.Count; $i++) {
        $settings["Cors__AllowedOrigins__$i"] = $CorsOrigins[$i]
    }
}

foreach ($key in $settings.Keys) {
    az webapp config appsettings set --resource-group $ResourceGroup --name $AppName --settings "$key=$($settings[$key])" | Out-Null
}

Write-Host "Stamp configuration complete."
