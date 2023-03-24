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
@description('The admin username for the server')
param sqlServerAdminLogin string

@secure()
@minLength(12)
@description('The admin password for the server')
param sqlServerAdminPassword string

@secure()
@description('The client ip of the agent. Used to allow migrations and will be removed at the end of deployment')
param clientIPOfAgent string

var sqlServerName = 'sql-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'

resource sqlServer 'Microsoft.Sql/servers@2022-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlServerAdminLogin
    administratorLoginPassword: sqlServerAdminPassword
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
  }
  tags: {
    intendedResourceName: 'sql-seelanstyres-${environment}'
  }

  resource azureInternalIPs 'firewallRules' = {
    name: 'AllowAllAzureInternalIPs'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }

  resource clientIP 'firewallRules' = {
    name: 'ClientIP'
    properties: {
      startIpAddress: clientIPOfAgent
      endIpAddress: clientIPOfAgent
    }
  }
}

resource seelansTyresAddressDb 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  parent: sqlServer
  name: 'SeelansTyresAddressDb'
  location: location
  sku: environment == 'dev' ? {
                                name: 'Basic'
                                tier: 'Basic'
                              } : {
                                name: 'GP_S_Gen5'
                                tier: 'GeneralPurpose'
                                family: 'Gen5'
                                capacity: 1
                              }
}

resource seelansTyresIdentityDb 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  parent: sqlServer
  name: 'SeelansTyresIdentityDb'
  location: location
  sku: environment == 'dev' ? {
                                name: 'Basic'
                                tier: 'Basic'
                              } : {
                                name: 'GP_S_Gen5'
                                tier: 'GeneralPurpose'
                                family: 'Gen5'
                                capacity: 1
                              }
}

resource seelansTyresOrderDb 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  parent: sqlServer
  name: 'SeelansTyresOrderDb'
  location: location
  sku: environment == 'dev' ? {
                                name: 'Basic'
                                tier: 'Basic'
                              } : {
                                name: 'GP_S_Gen5'
                                tier: 'GeneralPurpose'
                                family: 'Gen5'
                                capacity: 1
                              }
}

resource seelansTyresTyresDb 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  parent: sqlServer
  name: 'SeelansTyresTyresDb'
  location: location
  sku: environment == 'dev' ? {
                                name: 'Basic'
                                tier: 'Basic'
                              } : {
                                name: 'GP_S_Gen5'
                                tier: 'GeneralPurpose'
                                family: 'Gen5'
                                capacity: 1
                              }
}
