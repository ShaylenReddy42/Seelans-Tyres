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
@description('Your email address')
param emailAddressOfResponder string

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  name: 'rg-seelanstyres-${environment}'
}

module scheduledQueryRule '01-scheduled-query-rule/template.bicep' = {
  scope: resourceGroup
  name: 'scheduledQueryRule'
  params: {
    location: location
    environment: environment
    emailAddressOfResponder: emailAddressOfResponder
  }
}

module roleAssignmentToFunctionApp '02-role-assignment-to-function-app/template.bicep' = {
  scope: resourceGroup
  name: 'roleAssignmentToFunctionApp'
  params: {
    environment: environment
  }
}
