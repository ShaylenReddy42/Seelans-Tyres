@description('Specifies the location for resources.')
param location string = resourceGroup().location

@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

var redisName = 'redis-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'

resource redis 'Microsoft.Cache/redis@2022-06-01' = {
  name: redisName
  location: location
  properties: {
    sku: environment == 'dev' ? {
                                  name: 'Basic'
                                  family: 'C'
                                  capacity: 0
                                } : {
                                  name: 'Standard'
                                  family: 'C'
                                  capacity: 1
                                }
    enableNonSslPort: false
    redisVersion: '6'
  }
  tags: {
    intendedResourceName: 'redis-seelanstyres-${environment}'
  }
}

output redisName string = redisName
