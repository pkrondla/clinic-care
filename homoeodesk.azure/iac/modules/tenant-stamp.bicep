@description('Per-tenant stamp')
param environment string
param tenantKey string
param azureResourcePrefix string
param location string = resourceGroup().location
param apiSku string = 'F1'
param sqlSku string = 'Basic'
param staticWebSku string = 'Free'

module api 'app-service.bicep' = {
  name: 'tenant-api-${tenantKey}'
  params: {
    name: 'api-${azureResourcePrefix}-${environment}-${tenantKey}'
    location: location
    sku: apiSku
  }
}

module sql 'sql.bicep' = {
  name: 'tenant-sql-${tenantKey}'
  params: {
    serverName: 'sql-${azureResourcePrefix}-${environment}-${tenantKey}'
    databaseName: 'sqldb-${azureResourcePrefix}-${environment}-${tenantKey}'
    location: location
    sku: sqlSku
  }
}

module web 'static-web.bicep' = {
  name: 'tenant-web-${tenantKey}'
  params: {
    name: 'swa-${azureResourcePrefix}-${environment}-${tenantKey}'
    location: location
    sku: staticWebSku
  }
}

output apiHostName string = api.outputs.hostName
output webHostName string = web.outputs.defaultHostname
output sqlServerFqdn string = sql.outputs.serverFqdn
