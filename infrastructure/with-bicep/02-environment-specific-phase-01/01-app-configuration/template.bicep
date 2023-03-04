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

var appConfigurationSku = environment == 'dev' ? 'Free' : 'Standard'

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: appConfigurationName
  location: location
  sku: {
    name: appConfigurationSku
  }
  tags:{
    intendedResourceName: 'appcs-seelanstyres-${environment}'
  }
}

resource systemDegradedKeyValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2022-05-01' = {
  parent: appConfiguration
  name: 'SystemDegraded'
  properties: {
    contentType: 'application/json'
    value: 'false'
  }
}
