@description('Specifies the location for resources.')
param location string = resourceGroup().location

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

param existingApplicationInsightsName string

param existingFunctionAppName string

var actionGroupName = 'ag-systemdegraded-${environment}-${uniqueString(resourceGroup().id)}'

var scheduledQueryRuleName = 'sqrule-systemdegraded-${environment}-${uniqueString(resourceGroup().id)}'

// Needed to tie the scheduled query rule to it
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: existingApplicationInsightsName
}

// Needed to create the action group, and acts as a receiver
// listKeys() is used
// see https://learn.microsoft.com/en-us/rest/api/appservice/web-apps/list-host-keys
resource functionApp 'Microsoft.Web/sites@2022-03-01' existing = {
  name: existingFunctionAppName
}

resource actionGroup 'Microsoft.Insights/actionGroups@2022-06-01' = {
  name: actionGroupName
  location: 'Global'
  properties: {
    groupShortName: 'sysdegraded'
    enabled: true
    emailReceivers: [
      {
        name: 'myself'
        emailAddress: emailAddressOfResponder
        useCommonAlertSchema: false
      }
    ]
    azureFunctionReceivers: [
      {
        name: 'System Degraded Toggler'
        functionAppResourceId: functionApp.id
        functionName: 'TurnItOn'
        httpTriggerUrl: 'https://${functionApp.properties.defaultHostName}/api/turniton?code=${listKeys('${functionApp.id}/host/default', functionApp.apiVersion).masterKey}'
        useCommonAlertSchema: true
      }
    ]
  }
  tags: {
    intendedResourceName: 'ag-systemdegraded-${environment}'
  }
}

resource scheduledQueryRule 'Microsoft.Insights/scheduledqueryrules@2022-08-01-preview' = {
  name: scheduledQueryRuleName
  location: location
  properties: {
    displayName: scheduledQueryRuleName
    severity: 0
    enabled: true
    evaluationFrequency: 'PT5M'
    scopes: [
      applicationInsights.id
    ]
    targetResourceTypes: [
      'Microsoft.Insights/components'
    ]
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          query: 'customEvents\n| where name == "AspNetCoreHealthCheck"\n| where customMeasurements.["AspNetCoreHealthCheckStatus"] == 0\n| project\n    timestamp,\n    ApplicationName=customDimensions.["Assembly"],\n    ApplicationVersion=application_Version,\n    HealthCheckStatus=customMeasurements.["AspNetCoreHealthCheckStatus"],\n    HealthCheckDuration=customMeasurements.["AspNetCoreHealthCheckDuration"]\n'
          timeAggregation: 'Count'
          dimensions: []
          operator: 'GreaterThan'
          threshold: 3
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    autoMitigate: false
    actions: {
      actionGroups: [
        actionGroup.id
      ]
    }
  }
  tags: {
    intendedResourceName: 'sqrule-systemdegraded-${environment}'
  }
}
