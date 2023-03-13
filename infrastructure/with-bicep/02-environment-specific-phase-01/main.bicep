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
@description('The admin username for the server')
param sqlServerAdminLogin string

@secure()
@minLength(12)
@description('The admin password for the server')
param sqlServerAdminPassword string

@secure()
@description('The client ip of the agent. Used to allow migrations and will be removed at the end of deployment')
param clientIPOfAgent string

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
