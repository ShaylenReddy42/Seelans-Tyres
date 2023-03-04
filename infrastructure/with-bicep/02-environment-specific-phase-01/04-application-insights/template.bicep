@description('Specifies the location for resources.')
param location string = resourceGroup().location

@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

var logAnalyticsName = 'log-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'

var applicationInsightsName = 'appi-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  tags: {
    intendedResourceName: 'log-seelanstyres-${environment}'
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Redfield'
    WorkspaceResourceId: logAnalytics.id
  }
  tags: {
    intendedResourceName: 'appi-seelanstyres-${environment}'
  }
}
