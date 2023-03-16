targetScope = 'subscription'

@description('Specifies the location for resources.')
param location string = deployment().location

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-seelanstyres-agnostic'
  location: location
  tags: {
    environment: 'agnostic'
    managedWith: 'bicep'
    solution: 'seelanstyres'
  }
}

module queryPack '01-query-pack/template.bicep' = {
  scope: resourceGroup
  name: 'queryPack'
  params: {
    location: location
  }
}
