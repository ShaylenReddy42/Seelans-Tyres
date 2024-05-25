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
    body: 'AppEvents\r\n| where Name == "AspNetCoreHealthCheck"\r\n| where Measurements.["AspNetCoreHealthCheckStatus"] == 0\r\n| project\r\n    TimeGenerated,\r\n    ApplicationName=Properties.["Assembly"],\r\n    HealthCheckStatus=Measurements.["AspNetCoreHealthCheckStatus"],\r\n    HealthCheckDuration=Measurements.["AspNetCoreHealthCheckDuration"]'
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

resource matchElasticsearchAndKibanaExperienceQuery 'microsoft.operationalInsights/querypacks/queries@2019-09-01' = {
  parent: queryPack
  name: 'ae64d220-2476-4886-84fe-538eae3bab72'
  properties: {
    displayName: 'Match Elasticsearch and Kibana Experience'
    description: 'A very specific query to attempt to match the search experience when using Elasticsearch and Kibana'
    body: 'AppEvents\r\n| sort by TimeGenerated desc\r\n| project \r\n    TimeGenerated,\r\n    DescriptiveApplicationName=Properties.["Custom: Descriptive Application Name"],\r\n    RenderedMessage=Properties.["RenderedMessage"],\r\n    Properties=Properties'
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
