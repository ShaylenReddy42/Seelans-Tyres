@description('Specifies the location for resources.')
param location string = resourceGroup().location

@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

var appConfigurationName = 'appcs-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-09-01-preview' = {
  name: appConfigurationName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    softDeleteRetentionInDays: 1
  }
  tags:{
    intendedResourceName: 'appcs-seelanstyres-${environment}'
  }
}

resource systemDegradedKeyValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-09-01-preview' = {
  parent: appConfiguration
  name: 'SystemDegraded'
  properties: {
    contentType: 'application/json'
    value: 'false'
  }
}

output appConfigurationName string = appConfigurationName
