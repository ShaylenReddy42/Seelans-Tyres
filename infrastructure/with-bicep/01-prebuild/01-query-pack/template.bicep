@description('Specifies the location for resources.')
param location string = resourceGroup().location

var queryPackName = 'pack-seelanstyres-queries-${uniqueString(resourceGroup().id)}'

resource queryPack 'microsoft.operationalInsights/querypacks@2019-09-01' = {
  name: queryPackName
  location: location
  properties: {}
  tags: {
    intendedResourceName: 'pack-seelanstyres-queries'
  }
}

resource healthCheckStatusQuery 'microsoft.operationalInsights/querypacks/queries@2019-09-01' = {
  parent: queryPack
  name: '10e4a6c1-8e92-4b42-9940-84ef43af71c9'
  properties: {
    displayName: 'Health Check Status'
    description: 'Used to query for the health status according to the AspNetCore Health Checks specification'
    body: 'customEvents\r\n| where name == "AspNetCoreHealthCheck"\r\n| where customMeasurements.["AspNetCoreHealthCheckStatus"] == 0\r\n| project\r\n    timestamp,\r\n    ApplicationName=customDimensions.["Assembly"],\r\n    ApplicationVersion=application_Version,\r\n    HealthCheckStatus=customMeasurements.["AspNetCoreHealthCheckStatus"],\r\n    HealthCheckDuration=customMeasurements.["AspNetCoreHealthCheckDuration"]'
    related: {
      categories: []
      resourceTypes: [
        'microsoft.insights/components'
      ]
    }
    tags: {
      labels: [
        'Custom'
      ]
    }
  }
}
