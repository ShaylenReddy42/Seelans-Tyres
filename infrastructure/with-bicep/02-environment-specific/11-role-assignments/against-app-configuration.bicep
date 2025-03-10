@description('Name of the App Configuration instance to be granted permission to')
param appConfigurationName string

@description('Managed identity of the resource to be granted the role assignment')
param principalId string

@description('Id of the role as a guid')
param roleDefinitionId string

@description('Name of the role assignment [as a guid] created with the id of the assignee, the principal id, and the role definition id')
param roleAssignmentName string

// Needed to create the role assignment
// acts as the scope rather than a resource group or subscription scope
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-09-01-preview' existing = {
  name: appConfigurationName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: roleAssignmentName
  scope: appConfiguration
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
