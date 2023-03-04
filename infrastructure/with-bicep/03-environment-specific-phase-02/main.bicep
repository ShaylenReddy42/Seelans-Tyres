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

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  name: 'rg-seelanstyres-${environment}'
}

module functionApp '01-function-app/template.bicep' = {
  scope: resourceGroup
  name: 'functionApp'
  params: {
    location: location
    environment: environment
  }
}
