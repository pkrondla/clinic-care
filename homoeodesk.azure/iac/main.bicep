@description('HomoeoDesk Azure deployment entry')
param stampType string = 'global' // global | tenant
param environment string = 'dev'
param tenantKey string = 'demo'
param azureResourcePrefix string = 'tr-hd'
param location string = resourceGroup().location
param apiSku string = 'F1'
param sqlSku string = 'Basic'
param staticWebSku string = 'Free'

module globalStamp 'modules/global-stamp.bicep' = if (stampType == 'global') {
  name: 'global-stamp-${environment}'
  params: {
    environment: environment
    azureResourcePrefix: azureResourcePrefix
    location: location
    apiSku: apiSku
    sqlSku: sqlSku
    staticWebSku: staticWebSku
  }
}

module tenantStamp 'modules/tenant-stamp.bicep' = if (stampType == 'tenant') {
  name: 'tenant-stamp-${tenantKey}-${environment}'
  params: {
    environment: environment
    tenantKey: tenantKey
    azureResourcePrefix: azureResourcePrefix
    location: location
    apiSku: apiSku
    sqlSku: sqlSku
    staticWebSku: staticWebSku
  }
}

output apiHostName string = stampType == 'global' ? globalStamp!.outputs.apiHostName : tenantStamp!.outputs.apiHostName
output webHostName string = stampType == 'global' ? globalStamp!.outputs.webHostName : tenantStamp!.outputs.webHostName
