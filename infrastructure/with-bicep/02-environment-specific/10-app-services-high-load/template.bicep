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
@description('Used to Seed the Admin account to the database with the Administrator role')
param adminCredentialsEmail string

@secure()
@description('Password used to create the admin account')
param adminCredentialsPassword string

@secure()
@description('Email used to send emails')
param emailCredentialsEmail string

@secure()
@description('This is a generated app password')
param emailCredentialsPassword string

@secure()
@description('The health check endpoint starting with /')
param healthCheckEndpoint string

@secure()
@description('Used to authenticate the Mvc Frontend with IdentityServer4')
param mvcClientSecret string

@secure()
@description('Used to authenticate the WebBff Gateway with IdentityServer4')
param webBffClientSecret string

@secure()
@description('The admin username for the server')
param sqlServerAdminLogin string

@secure()
@minLength(12)
@description('The admin password for the server')
param sqlServerAdminPassword string

param existingAppConfigurationName string

param existingApplicationInsightsName string

param existingServiceBusNamespaceName string

param existingRedisName string

param existingSqlServerName string

param existingStorageAccountName string

var appServicePlanName = 'plan-seelantyres-hl-${environment}-${uniqueString(resourceGroup().id)}'

// Needed to extract its connection string for app settings
// listKeys() is used
// see https://learn.microsoft.com/en-us/rest/api/appconfiguration/stable/configuration-stores/list-keys?tabs=HTTP
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-09-01-preview' existing = {
  name: existingAppConfigurationName
}

// Needed to extract its connection string for app setting
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: existingApplicationInsightsName
}

// Needed to extract its connection string for app settings
// using the RootManageSharedAccessKey authorization rule
// created by default when a namespace is created
// listKeys() is used
// see https://learn.microsoft.com/en-us/rest/api/servicebus/stable/namespaces-authorization-rules/list-keys?tabs=HTTP
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: existingServiceBusNamespaceName
}

// Needed to build the connection string for app settings
// listKeys() is used
// see https://learn.microsoft.com/en-us/rest/api/redis/redis/list-keys?tabs=HTTP
resource redis 'Microsoft.Cache/redis@2023-08-01' existing = {
  name: existingRedisName
}

// Needed to build the connection string to the database
// using its fully qualified domain name
// part of app settings
resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
  name: existingSqlServerName
}

// Needed to build a connection string for app settings
// listKeys() is used
// see https://learn.microsoft.com/en-us/rest/api/storagerp/storage-accounts/list-keys?tabs=HTTP
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-04-01' existing = {
  name: existingStorageAccountName
}

var databaseConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=$(Database:Name);User ID=${sqlServerAdminLogin};Password=${sqlServerAdminPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;'

var redisConnectionString = '${redis.properties.hostName}:${redis.properties.sslPort},password=${redis.listKeys().primaryKey},ssl=True,abortConnect=False'

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: environment == 'dev' ? {
                                tier: 'PremiumV3'
                                name: 'P1V3'
                              } : {
                                tier: 'PremiumV3'
                                name: 'P2V3'
                              }
  properties: {
    reserved: true
  }
  tags: {
    intendedResourceName: 'plan-seelanstyres-hl-${environment}'
  }
}

resource mvc 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-seelanstyres-mvc-${environment}-${uniqueString(resourceGroup().id)}'
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
          name: 'ClientCredentials__ClientSecret'
          value: mvcClientSecret
        }
        {
          name: 'ConnectionStrings__AzureStorageAccount'
          value: storageAccountConnectionString
        }
        {
          name: 'EmailCredentials__Email'
          value: emailCredentialsEmail
        }
        {
          name: 'EmailCredentials__Password'
          value: emailCredentialsPassword
        }
        {
          name: 'IdentityServer'
          value: 'https://app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'InAzure'
          value: 'true'
        }
        {
          name: 'WebBffUrl'
          value: 'https://app-seelanstyres-webbff-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'Redis__Enabled'
          value: 'true'
        }
        {
          name: 'Redis__ConnectionString'
          value: redisConnectionString
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
      linuxFxVersion: 'DOTNETCORE|9.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-mvc-${environment}'
  }
}

resource webBff 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-seelanstyres-webbff-${environment}-${uniqueString(resourceGroup().id)}'
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
          name: 'ClientCredentials__ClientSecret'
          value: webBffClientSecret
        }
        {
          name: 'IdentityServer'
          value: 'https://app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'envBaseUrl'
          value: 'https://app-seelanstyres-webbff-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'envAddressServiceScheme'
          value: 'https'
        }
        {
          name: 'envAddressServiceHost'
          value: 'app-seelanstyres-addressservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'envAddressServicePort'
          value: '443'
        }
        {
          name: 'envCustomerServiceScheme'
          value: 'https'
        }
        {
          name: 'envCustomerServiceHost'
          value: 'app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'envCustomerServicePort'
          value: '443'
        }
        {
          name: 'envOrderServiceScheme'
          value: 'https'
        }
        {
          name: 'envOrderServiceHost'
          value: 'app-seelanstyres-orderservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'envOrderServicePort'
          value: '443'
        }
        {
          name: 'envTyresServiceScheme'
          value: 'https'
        }
        {
          name: 'envTyresServiceHost'
          value: 'app-seelanstyres-tyresservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'envTyresServicePort'
          value: '443'
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
      linuxFxVersion: 'DOTNETCORE|9.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-webbff-${environment}'
  }
}

resource identityService 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}'
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
          name: 'BaseUrl'
          value: 'https://app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'AdminCredentials__Email'
          value: adminCredentialsEmail
        }
        {
          name: 'AdminCredentials__Password'
          value: adminCredentialsPassword
        }
        {
          name: 'Clients__SeelansTyresMvcClient__ClientSecret'
          value: mvcClientSecret
        }
        {
          name: 'Clients__SeelansTyresMvcClient__Url'
          value: 'https://app-seelanstyres-mvc-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'Clients__SeelansTyresWebBffClient__ClientSecret'
          value: webBffClientSecret
        }
        {
          name: 'Database__ConnectionString'
          value: databaseConnectionString
        }
        {
          name: 'InAzure'
          value: 'true'
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
      linuxFxVersion: 'DOTNETCORE|9.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-identityservice-${environment}'
  }
}
