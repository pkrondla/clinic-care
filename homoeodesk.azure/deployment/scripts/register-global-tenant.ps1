param(
    [Parameter(Mandatory = $true)][string]$GlobalApiUrl,
    [Parameter(Mandatory = $true)][string]$TenantKey,
    [Parameter(Mandatory = $true)][int]$TenantId,
    [Parameter(Mandatory = $true)][string]$ConnectionString,
    [Parameter(Mandatory = $true)][string]$Subdomain
)

$body = @{
    name = $TenantKey
    subdomain = $Subdomain
    tenantId = $TenantId
    connectionString = $ConnectionString
} | ConvertTo-Json

Write-Host "Register tenant $TenantKey with global API at $GlobalApiUrl"
Invoke-RestMethod -Method Post -Uri "$GlobalApiUrl/api/global/organizations" -Body $body -ContentType "application/json"
Write-Host "Done."
