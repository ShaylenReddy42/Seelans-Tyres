targetScope = 'subscription'

@description('Specifies the location for resources.')
param location string = deployment().location

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
@description('The client ip of the agent. Used to allow migrations and will be removed at the end of deployment')
param clientIPOfAgent string

@secure()
@description('Your email address')
param emailAddressOfResponder string

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

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-seelanstyres-${environment}'
  location: location
  tags: {
    environment: environment
    managedWith: 'bicep'
    solution: 'seelanstyres'
  }
}

module appConfiguration '01-app-configuration/template.bicep' = {
  scope: resourceGroup
  name: 'appConfiguration'
  params: {
    location: location
    environment: environment
  }
}

module serviceBusNamespace '02-service-bus/template.bicep' = {
  scope: resourceGroup
  name: 'serviceBusNamespace'
  params: {
    location: location
    environment: environment
  }
}

module storageAccount '03-storage-account/template.bicep' = {
  scope: resourceGroup
  name: 'storageAccount'
  params: {
    location: location
    environment: environment
  }
}

module applicationInsights '04-application-insights/template.bicep' = {
  scope: resourceGroup
  name: 'applicationInsights'
  params: {
    location: location
    environment: environment
  }
}

module redis '05-redis/template.bicep' = {
  scope: resourceGroup
  name: 'redis'
  params: {
    location: location
    environment: environment
  }
}

module sqlServer '06-sql-server/template.bicep' = {
  scope: resourceGroup
  name: 'sqlServer'
  params: {
    location: location
    environment: environment
    clientIPOfAgent: clientIPOfAgent
    sqlServerAdminLogin: sqlServerAdminLogin
    sqlServerAdminPassword: sqlServerAdminPassword
  }
}

module functionApp '07-function-app/template.bicep' = {
  scope: resourceGroup
  name: 'functionApp'
  params: {
    location: location
    environment: environment
    
    existingAppConfigurationName: appConfiguration.outputs.appConfigurationName
    existingApplicationInsightsName: applicationInsights.outputs.applicationInsightsName
    existingStorageAccountName: storageAccount.outputs.storageAccountName
  }
}

module appServicesLowLoad '08-app-services-low-load/template.bicep' = {
  scope: resourceGroup
  name: 'appServicesLowLoad'
  params: {
    location: location
    environment: environment
    healthCheckEndpoint: healthCheckEndpoint
    sqlServerAdminLogin: sqlServerAdminLogin
    sqlServerAdminPassword: sqlServerAdminPassword

    existingAppConfigurationName: appConfiguration.outputs.appConfigurationName
    existingApplicationInsightsName: applicationInsights.outputs.applicationInsightsName
    existingServiceBusNamespaceName: serviceBusNamespace.outputs.serviceBusNamespaceName
    existingSqlServerName: sqlServer.outputs.sqlServerName
  }
}

module appServicesModerateLoad '09-app-services-moderate-load/template.bicep' = {
  scope: resourceGroup
  name: 'appServicesModerateLoad'
  params: {
    location: location
    environment: environment
    healthCheckEndpoint: healthCheckEndpoint
    sqlServerAdminLogin: sqlServerAdminLogin
    sqlServerAdminPassword: sqlServerAdminPassword

    existingAppConfigurationName: appConfiguration.outputs.appConfigurationName
    existingApplicationInsightsName: applicationInsights.outputs.applicationInsightsName
    existingServiceBusNamespaceName: serviceBusNamespace.outputs.serviceBusNamespaceName
    existingSqlServerName: sqlServer.outputs.sqlServerName
  }
}

module appServicesHighLoad '10-app-services-high-load/template.bicep' = {
  scope: resourceGroup
  name: 'appServicesHighLoad'
  params: {
    location: location
    environment: environment
    adminCredentialsEmail: adminCredentialsEmail
    adminCredentialsPassword: adminCredentialsPassword
    emailCredentialsEmail: emailCredentialsEmail
    emailCredentialsPassword: emailCredentialsPassword
    healthCheckEndpoint: healthCheckEndpoint
    mvcClientSecret: mvcClientSecret
    webBffClientSecret: webBffClientSecret
    sqlServerAdminLogin: sqlServerAdminLogin
    sqlServerAdminPassword: sqlServerAdminPassword

    existingAppConfigurationName: appConfiguration.outputs.appConfigurationName
    existingApplicationInsightsName: applicationInsights.outputs.applicationInsightsName
    existingServiceBusNamespaceName: serviceBusNamespace.outputs.serviceBusNamespaceName
    existingRedisName: redis.outputs.redisName
    existingSqlServerName: sqlServer.outputs.sqlServerName
    existingStorageAccountName: storageAccount.outputs.storageAccountName
  }
}

module scheduledQueryRule '12-scheduled-query-rule/template.bicep' = {
  scope: resourceGroup
  name: 'scheduledQueryRule'
  params: {
    location: location
    environment: environment
    emailAddressOfResponder: emailAddressOfResponder

    existingApplicationInsightsName: applicationInsights.outputs.applicationInsightsName
    existingFunctionAppName: functionApp.outputs.functionAppName
  }
}

module roleAssignmentToFunctionApp '13-role-assignment-to-function-app/template.bicep' = {
  scope: resourceGroup
  name: 'roleAssignmentToFunctionApp'
  params: {
    existingAppConfigurationName: appConfiguration.outputs.appConfigurationName
    existingFunctionAppName: functionApp.outputs.functionAppName
  }
}
