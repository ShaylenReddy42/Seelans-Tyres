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
@description('Used to authenticate the MvcBff Gateway with IdentityServer4')
param mvcBffClientSecret string

@secure()
@description('The admin username for the server')
param sqlServerAdminLogin string

@secure()
@minLength(12)
@description('The admin password for the server')
param sqlServerAdminPassword string

var appServicePlanName = 'plan-seelantyres-hl-${environment}-${uniqueString(resourceGroup().id)}'

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: 'appcs-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'appi-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: 'sb-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

resource redis 'Microsoft.Cache/redis@2022-06-01' existing = {
  name: 'redis-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

resource sqlServer 'Microsoft.Sql/servers@2022-08-01-preview' existing = {
  name: 'sql-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: substring('stseelanstyres${environment}${uniqueString(resourceGroup().id)}', 0, 24)
}

var databaseConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=$(Database:Name);User ID=${sqlServerAdminLogin};Password=${sqlServerAdminPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;'

var redisConnectionString = '${redis.properties.hostName}:${redis.properties.sslPort},password=${redis.listKeys().primaryKey},ssl=True,abortConnect=False'

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: environment == 'dev' ? {
                                tier: 'Standard'
                                name: 'S2'
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

resource mvc 'Microsoft.Web/sites@2022-03-01' = {
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
          name: 'MvcBffUrl'
          value: 'https://app-seelanstyres-mvcbff-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
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
      linuxFxVersion: 'DOTNETCORE|7.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-mvc-${environment}'
  }
}

resource mvcBff 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-seelanstyres-mvcbff-${environment}-${uniqueString(resourceGroup().id)}'
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
          value: mvcBffClientSecret
        }
        {
          name: 'IdentityServer'
          value: 'https://app-seelanstyres-identityservice-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
        }
        {
          name: 'envBaseUrl'
          value: 'https://app-seelanstyres-mvcbff-${environment}-${uniqueString(resourceGroup().id)}.azurewebsites.net'
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
      linuxFxVersion: 'DOTNETCORE|7.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-mvcbff-${environment}'
  }
}

resource identityService 'Microsoft.Web/sites@2022-03-01' = {
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
          name: 'Clients__SeelansTyresMvcBffClient__ClientSecret'
          value: mvcBffClientSecret
        }
        {
          name: 'Database__ConnectionString'
          value: databaseConnectionString
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
      linuxFxVersion: 'DOTNETCORE|6.0'
    }
  }
  tags: {
    intendedResourceName: 'app-seelanstyres-identityservice-${environment}'
  }
}
