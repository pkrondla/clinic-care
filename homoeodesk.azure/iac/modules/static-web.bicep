@description('Static Web App for HomoeoDesk frontends')
param name string
param location string = 'centralus'
param sku string = 'Free'

resource staticWeb 'Microsoft.Web/staticSites@2023-01-01' = {
  name: name
  location: location
  sku: {
    name: sku
    tier: sku
  }
  properties: {}
}

output defaultHostname string = staticWeb.properties.defaultHostname
