targetScope = 'subscription'

@description('Required. The name of the environment.')
param environment string

@description('Required. The location for the resources.')
param location string

@description('Required. The name of the resource group.')
param resourceGroupName string

@description('Required.')
param eventHubNamespaceName string

@description('Required.')
param eventGridNamespaceName string

var tags = union(loadJsonContent('../../templates/tags/tags.json'), {
  Environment: environment
})

param eventHubs array = [
  {
    name: 'consumer1'
    partitionCount: 1
  }
]

module resourceGroup 'br/public:avm/res/resources/resource-group:0.4.1' = {
  name: '${uniqueString(deployment().name, location)}-resourceGroup'
  params: {
    name: resourceGroupName
    location: location
    tags: tags
  }
}

module eventGridNamespace 'br/public:avm/res/event-grid/namespace:0.7.2' = {
  scope: az.resourceGroup(resourceGroupName)
  name: 'eventGridNamespace'
  params: {
    name: eventGridNamespaceName
    location: resourceGroup.outputs.location
    managedIdentities: {
      systemAssigned: true
    }
    topics: [
      {
        name: 'canonical1'
        eventRetentionInDays: 1
        eventSubscriptions: [
          {
            deliveryConfiguration: {
              deliveryMode: 'Push'
              push: {
                deliveryWithResourceIdentity: {
                  destination: {
                    endpointType: 'EventHub'
                    properties: {
                      resourceId: eventHubNamespace.outputs.eventHubResourceIds[0]
                    }
                  }
                  identity: {
                    type: 'systemAssigned'
                  }
                }
                eventTimeToLive: 'P1D'
                maxDeliveryCount: 10
              }
            }
            filtersConfiguration: {
              includedEventTypes: [
                'com.ext.consumer.1.event.eventcreated'
              ]
              filters: [
                {
                  values: [
                    [
                      json('5')
                      json('15')
                    ]
                  ]
                  operatorType: 'NumberInRange'
                  key: 'data.key1'
                }
                {
                  values: [
                    [
                      json('35')
                      json('40')
                    ]
                  ]
                  operatorType: 'NumberInRange'
                  key: 'data.latitude'
                }
              ]
            }
            name: '${eventHubNamespaceName}-${eventHubs[0].name}'
          }
        ]
      }
    ]
  }
}

module eventHubNamespace 'br/public:avm/res/event-hub/namespace:0.12.0' = {
  scope: az.resourceGroup(resourceGroupName)
  name: 'eventHubNamespace'
  params: {
    name: eventHubNamespaceName
    location: resourceGroup.outputs.location
    tags: tags
    managedIdentities: {
      systemAssigned: true
    }
    publicNetworkAccess: 'Enabled'
    skuCapacity: 1
    skuName: 'Standard'
    disableLocalAuth: false
    eventhubs: [
      for eventHub in eventHubs: {
        name: eventHub.name
        partitionCount: eventHub.partitionCount
        messageRetentionInDays: 1
        retentionDescriptionCleanupPolicy: 'Delete'
        retentionDescriptionRetentionTimeInHours: 1
      }
    ]
  }
}
