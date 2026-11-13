@description('Azure region')
param location string = resourceGroup().location
@description('Short globally unique resource prefix')
@minLength(3)
@maxLength(10)
param namePrefix string
@allowed(['Basic', 'Standard'])
param registrySku string = 'Basic'

var suffix = uniqueString(resourceGroup().id)
var registryName = toLower('${namePrefix}${suffix}')

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${namePrefix}-logs'
  location: location
  properties: { sku: { name: 'PerGB2018' }, retentionInDays: 30 }
}

resource managedEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${namePrefix}-aca-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: registryName
  location: location
  sku: { name: registrySku }
  properties: { adminUserEnabled: false }
}

output containerRegistryName string = registry.name
output containerRegistryLoginServer string = registry.properties.loginServer
output containerAppsEnvironmentName string = managedEnvironment.name
output logAnalyticsWorkspaceName string = logAnalytics.name
