param existingAppConfigurationName string

param existingFunctionAppName string

// Needed to extract its name to create the role assignment
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: existingAppConfigurationName
}

// Needed for its id and principalId to create the role assignment
resource functionApp 'Microsoft.Web/sites@2022-03-01' existing = {
  name: existingFunctionAppName
}

// Got this from https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
// pressed [ctrl+f] to find 'app configuration data owner' 
resource appConfigurationDataOwnerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '5ae67dd6-50cb-40e7-96ff-dc2bfa4b606b'
}

module appConfigurationRoleAssignment '../11-role-assignments/against-app-configuration.bicep' = {
  name: 'appConfigurationRoleAssignment'
  params: {
    appConfigurationName: appConfiguration.name
    principalId: functionApp.identity.principalId
    roleDefinitionId: appConfigurationDataOwnerRoleDefinition.id
    roleAssignmentName: guid(functionApp.id, functionApp.identity.principalId, appConfigurationDataOwnerRoleDefinition.id)
  }
}
