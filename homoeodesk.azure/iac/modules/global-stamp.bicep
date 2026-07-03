@description('Global control-plane stamp')
param environment string
param azureResourcePrefix string
param location string = resourceGroup().location
param apiSku string = 'F1'
param sqlSku string = 'Basic'
param staticWebSku string = 'Free'

module api 'app-service.bicep' = {
  name: 'global-api'
  params: {
    name: 'api-${azureResourcePrefix}-${environment}-global'
    location: location
    sku: apiSku
  }
}

module sql 'sql.bicep' = {
  name: 'global-sql'
  params: {
    serverName: 'sql-${azureResourcePrefix}-${environment}-global'
    databaseName: 'sqldb-homoeodesk-${environment}-global'
    location: location
    sku: sqlSku
  }
}

module web 'static-web.bicep' = {
  name: 'global-web'
  params: {
    name: 'swa-${azureResourcePrefix}-${environment}-global'
    location: location
    sku: staticWebSku
  }
}

module vault 'key-vault.bicep' = {
  name: 'global-kv'
  params: {
    name: 'kv-${azureResourcePrefix}-${environment}-g'
    location: location
  }
}

output apiHostName string = api.outputs.hostName
output webHostName string = web.outputs.defaultHostname
output sqlServerFqdn string = sql.outputs.serverFqdn
output keyVaultUri string = vault.outputs.vaultUri
