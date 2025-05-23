# Event Grid / Hub Subscription and Filter 

The artifacts that accompany a spike around how Event Hub can subscribe to Event Grid. Also, how Event Grid supports the ability to filter based on the Clod Events Schema. All in a `.devcontainer`.

To deploy (from the container):

``` bash
az deployment sub create `
--name "capgemini-eds-01.main-$(Get-Date -Format 'yyyyMMdd')-$(Get-Date -Format 'HHmmss')" `
--location westeurope `
-f "/workspace/deploy/bicep/main.bicep" `
-p "/workspace/deploy/bicep/env/dev.bicepparam"
```

To get the hostname of the Event Grid for the REST POST:

`az eventgrid namespace show -g rg-workload-westeu-dev-01 -n evgns-workload-westeu-dev-01 --query "topicsConfiguration.hostname" --output tsv`

To get an access token for the REST POST to Event Grid (I also keep forgetting how to generate an access token to access a resource in Azure):

`az account get-access-token --resource https://eventgrid.azure.net/ --query accessToken -o tsv`
