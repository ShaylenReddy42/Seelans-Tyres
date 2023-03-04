@description('Specifies the location for resources.')
param location string = resourceGroup().location

@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

var serviceBusNamespaceName = 'sb-seelanstyres-${environment}-${uniqueString(resourceGroup().id)}'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
  }
  tags:{
    intendedResourceName: 'sb-seelanstyres-${environment}'
  }
}

resource sbt_seelanstyres_deleteaccount 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'sbt-seelanstyres-deleteaccount'
  properties: {
    maxMessageSizeInKilobytes: 256
    defaultMessageTimeToLive: 'P14D'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    status: 'Active'
    supportOrdering: true
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }

  resource seelanstyres_deleteaccount_addressservice 'subscriptions' = {
    name: 'seelanstyres-deleteaccount-addressservice'
    properties: {
      isClientAffine: false
      lockDuration: 'PT30S'
      requiresSession: false
      defaultMessageTimeToLive: 'P14D'
      deadLetteringOnMessageExpiration: false
      deadLetteringOnFilterEvaluationExceptions: false
      maxDeliveryCount: 1
      status: 'Active'
      enableBatchedOperations: true
      autoDeleteOnIdle: 'P10675198DT2H48M5.477S'
    }
  }
  
  resource seelanstyres_deleteaccount_orderservice 'subscriptions' = {
    name: 'seelanstyres-deleteaccount-orderservice'
    properties: {
      isClientAffine: false
      lockDuration: 'PT30S'
      requiresSession: false
      defaultMessageTimeToLive: 'P14D'
      deadLetteringOnMessageExpiration: false
      deadLetteringOnFilterEvaluationExceptions: false
      maxDeliveryCount: 1
      status: 'Active'
      enableBatchedOperations: true
      autoDeleteOnIdle: 'P10675198DT2H48M5.477S'
    }
  }
}

resource sbt_seelanstyres_updateaccount 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'sbt-seelanstyres-updateaccount'
  properties: {
    maxMessageSizeInKilobytes: 256
    defaultMessageTimeToLive: 'P14D'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    status: 'Active'
    supportOrdering: true
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }

  resource seelanstyres_updateaccount_orderservice 'subscriptions' = {
    name: 'seelanstyres-updateaccount-orderservice'
    properties: {
      isClientAffine: false
      lockDuration: 'PT30S'
      requiresSession: false
      defaultMessageTimeToLive: 'P14D'
      deadLetteringOnMessageExpiration: false
      deadLetteringOnFilterEvaluationExceptions: false
      maxDeliveryCount: 1
      status: 'Active'
      enableBatchedOperations: true
      autoDeleteOnIdle: 'P10675198DT2H48M5.477S'
    }
  }
}

resource sbt_seelanstyres_updatetyre 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'sbt-seelanstyres-updatetyre'
  properties: {
    maxMessageSizeInKilobytes: 256
    defaultMessageTimeToLive: 'P14D'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    status: 'Active'
    supportOrdering: true
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }

  resource seelanstyres_updatetyre_orderservice 'subscriptions' = {
    name: 'seelanstyres-updatetyre-orderservice'
    properties: {
      isClientAffine: false
      lockDuration: 'PT30S'
      requiresSession: false
      defaultMessageTimeToLive: 'P14D'
      deadLetteringOnMessageExpiration: false
      deadLetteringOnFilterEvaluationExceptions: false
      maxDeliveryCount: 1
      status: 'Active'
      enableBatchedOperations: true
      autoDeleteOnIdle: 'P10675198DT2H48M5.477S'
    }
  }
}
