targetScope = 'subscription'

@description('Specifies the location for resources.')
param location string = deployment().location

@allowed([
  'Basic'
  'Standard'
  'Premium'
])
@description('Specifies the sku name for the container registry')
param containerRegistrySku string = 'Basic'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-seelanstyres-agnostic'
  location: location
  tags: {
    environment: 'agnostic'
    managedWith: 'bicep'
    solution: 'seelanstyres'
  }
}

module containerRegistry '01-container-registry/template.bicep' = {
  scope: resourceGroup
  name: 'containerRegistry'
  params: {
    location: location
    containerRegistrySku: containerRegistrySku
  }
}
