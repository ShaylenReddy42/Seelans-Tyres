@description('Specifies the location for resources.')
param location string = resourceGroup().location

@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

param existingAppConfigurationName string

param existingApplicationInsightsName string

param existingStorageAccountName string

var appServicePlanName = 'plan-systemdegradedtoggler-${environment}-${uniqueString(resourceGroup().id)}'

var functionAppName = 'func-systemdegradedtoggler-${environment}-${uniqueString(resourceGroup().id)}'

// Needed to extract the name of the instance,
// part of the functionapp's app settings
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-09-01-preview' existing = {
  name: existingAppConfigurationName
}

// Needed to instrument the functionapp,
// part of the functionapp's app settings
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: existingApplicationInsightsName
}

// Needed to configure storage for the functionapp
// part of the functionapp's app settings
// listKeys() is used
// see https://learn.microsoft.com/en-us/rest/api/storagerp/storage-accounts/list-keys?tabs=HTTP
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-04-01' existing = {
  name: existingStorageAccountName
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    size: 'Y1'
    family: 'Y'
  }
  properties: {
    reserved: true
  }
  tags: {
    intendedResourceName: 'plan-systemdegradedtoggler-${environment}'
  }
}

resource systemDegradedFunctionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  kind: 'functionapp,linux'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    serverFarmId: appServicePlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      appSettings: [
        {
          name: 'AzureAppConfigName'
          value: appConfiguration.name
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED'
          value: '1'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: '${toLower(functionAppName)}be79'
        }
      ]
      use32BitWorkerProcess: false
      linuxFxVersion: 'DOTNET-ISOLATED|9'
    }
  }
  tags: {
    intendedResourceName: 'func-systemdegradedtoggler-${environment}'
  }
}

output functionAppName string = functionAppName
