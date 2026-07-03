param serverName string
param databaseName string
param location string
param sku string = 'Basic'

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: 'homoeodeskadmin'
    administratorLoginPassword: 'ChangeMe-In-KeyVault!'
    version: '12.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: sku
    tier: sku
  }
}

output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
