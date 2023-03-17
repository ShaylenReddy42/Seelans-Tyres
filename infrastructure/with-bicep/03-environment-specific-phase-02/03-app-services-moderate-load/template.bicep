@description('Specifies the location for resources.')
param location string = resourceGroup().location

@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

@secure()
@description('The health check endpoint starting with /')
param healthCheckEndpoint string

@secure()
@description('The admin username for the server')
param sqlServerAdminLogin string

@secure()
@minLength(12)
@description('The admin password for the server')
param sqlServerAdminPassword string

var appServicePlanName = 'plan-seelantyres-ml-${environment}-${uniqueString(resourceGroup().id)}'

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: 'appcs-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'appi-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: 'sb-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

resource sqlServer 'Microsoft.Sql/servers@2022-08-01-preview' existing = {
  name: 'sql-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

var databaseConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=$(Database:Name);User ID=${sqlServerAdminLogin};Password=${sqlServerAdminPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: environment == 'dev' ? {
                                tier: 'Basic'
                                name: 'B2'
                              } : {
                                tier: 'Standard'
                                name: 'S2'
                              }
  properties: {
    reserved: true
  }
  tags: {
    intendedResourceName: 'plan-seelanstyres-ml-${environment}'
  }
}

resource addressService 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-seelanstyres-addressservice-${environment}-${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureAppConfig__Enabled'
          value: 'true'
        }
        {
          name: 'AzureAppConfig__ConnectionString'
          value: appConfiguration.listKeys().value[0].connectionString
        }
        {
          name: 'AppInsights__Enabled'
          value: 'true'
        }
        {
          name: 'AppInsights__ConnectionString'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'Database__ConnectionString'
          value: databaseConnectionString
        }
        {
          name: 'IdentityServer'
          value: 'https://app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'HealthCheckEndpoint'
          value: healthCheckEndpoint
        }
      ]
      cors: {
        allowedOrigins: [
          'https://*.azurewebsites.net'
        ]
      }
      healthCheckPath: '${healthCheckEndpoint}/liveness'
      use32BitWorkerProcess: false
      linuxFxVersion: 'DOTNETCORE|7.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-addressservice-${environment}'
  }
}

resource orderService 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-seelanstyres-orderservice-${environment}-${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureAppConfig__Enabled'
          value: 'true'
        }
        {
          name: 'AzureAppConfig__ConnectionString'
          value: appConfiguration.listKeys().value[0].connectionString
        }
        {
          name: 'AppInsights__Enabled'
          value: 'true'
        }
        {
          name: 'AppInsights__ConnectionString'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'Database__ConnectionString'
          value: databaseConnectionString
        }
        {
          name: 'IdentityServer'
          value: 'https://app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'HealthCheckEndpoint'
          value: healthCheckEndpoint
        }
      ]
      cors: {
        allowedOrigins: [
          'https://*.azurewebsites.net'
        ]
      }
      healthCheckPath: '${healthCheckEndpoint}/liveness'
      use32BitWorkerProcess: false
      linuxFxVersion: 'DOTNETCORE|7.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-orderservice-${environment}'
  }
}

resource tyresService 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-seelanstyres-tyresservice-${environment}-${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureAppConfig__Enabled'
          value: 'true'
        }
        {
          name: 'AzureAppConfig__ConnectionString'
          value: appConfiguration.listKeys().value[0].connectionString
        }
        {
          name: 'AppInsights__Enabled'
          value: 'true'
        }
        {
          name: 'AppInsights__ConnectionString'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'Database__ConnectionString'
          value: databaseConnectionString
        }
        {
          name: 'IdentityServer'
          value: 'https://app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'AzureServiceBus__ConnectionString'
          value: '${listKeys('${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBus.apiVersion).primaryConnectionString}'
        }
        {
          name: 'HealthCheckEndpoint'
          value: healthCheckEndpoint
        }
      ]
      cors: {
        allowedOrigins: [
          'https://*.azurewebsites.net'
        ]
      }
      healthCheckPath: '${healthCheckEndpoint}/liveness'
      use32BitWorkerProcess: false
      linuxFxVersion: 'DOTNETCORE|7.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-tyresservice-${environment}'
  }
}
