@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

// Needed to extract its name to create the role assignment
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: 'appcs-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'
}

// Needed for its id and principalId to create the role assignment
resource functionApp 'Microsoft.Web/sites@2022-03-01' existing = {
  name: 'func-systemdegradedtoggler-${environment}-${uniqueString(resourceGroup().id)}'
}

// Got this from https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
// pressed [ctrl+f] to find 'app configuration data owner' 
resource appConfigurationDataOwnerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '5ae67dd6-50cb-40e7-96ff-dc2bfa4b606b'
}

module appConfigurationRoleAssignment '../00-role-assignments/against-app-configuration.bicep' = {
  name: 'appConfigurationRoleAssignment'
  params: {
    appConfigurationName: appConfiguration.name
    principalId: functionApp.identity.principalId
    roleDefinitionId: appConfigurationDataOwnerRoleDefinition.id
    roleAssignmentName: guid(functionApp.id, functionApp.identity.principalId, appConfigurationDataOwnerRoleDefinition.id)
  }
}
