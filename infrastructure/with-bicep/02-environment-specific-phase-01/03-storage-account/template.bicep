@description('Specifies the location for resources.')
param location string = resourceGroup().location

@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

var storageAccountName = substring('stseelanstyres${environment}${uniqueString(resourceGroup().id)}', 0, 24)

var storageAccountSku = environment == 'dev' ? 'Standard_LRS' : 'Standard_ZRS'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: storageAccountSku
  }
  tags: {
    intendedResourceName: 'st-seelanstyres-${environment}'
  }
}
