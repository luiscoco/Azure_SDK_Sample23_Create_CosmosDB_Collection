using System;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;

//1. Obtaine Azure credentials
TokenCredential cred = new DefaultAzureCredential();

//2. Azure Authentication
ArmClient client = new ArmClient(cred);

//3. Set the Azure SubscriptionId, the ResourceGroupName where the Azure CosmosDB is located and the Azure CosmosDB account name
string subscriptionId = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
string resourceGroupName = "rg1";
string accountName = "newcosmosdbwithazuresdk";
string databaseName = "ToDoList";

//
ResourceIdentifier cosmosDBSqlDatabaseResourceId = CosmosDBSqlDatabaseResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, accountName, databaseName);
CosmosDBSqlDatabaseResource cosmosDBSqlDatabase = client.GetCosmosDBSqlDatabaseResource(cosmosDBSqlDatabaseResourceId);

// get the collection of this CosmosDBSqlContainerResource
CosmosDBSqlContainerCollection collection = cosmosDBSqlDatabase.GetCosmosDBSqlContainers();

// invoke the operation
string containerName = "items";
CosmosDBSqlContainerCreateOrUpdateContent content = new CosmosDBSqlContainerCreateOrUpdateContent(new AzureLocation("WestEurope"), new CosmosDBSqlContainerResourceInfo(containerName)
{
    IndexingPolicy = new CosmosDBIndexingPolicy()
    {
        IsAutomatic = true,
        IndexingMode = CosmosDBIndexingMode.Consistent,
        IncludedPaths =
{
new CosmosDBIncludedPath()
{
Path = "/*",
Indexes =
{
new CosmosDBPathIndexes()
{
DataType = CosmosDBDataType.String,
Precision = -1,
Kind = CosmosDBIndexKind.Range,
},new CosmosDBPathIndexes()
{
DataType = CosmosDBDataType.Number,
Precision = -1,
Kind = CosmosDBIndexKind.Range,
}
},
}
},
        ExcludedPaths =
{
},
    },
    PartitionKey = new CosmosDBContainerPartitionKey()
    {
        Paths =
{
"/AccountNumber"
},
        Kind = CosmosDBPartitionKind.Hash,
    },
    DefaultTtl = 100,
    UniqueKeys =
{
new CosmosDBUniqueKey()
{
Paths =
{
"/testPath"
},
}
},
    ConflictResolutionPolicy = new ConflictResolutionPolicy()
    {
        Mode = ConflictResolutionMode.LastWriterWins,
        ConflictResolutionPath = "/path",
    },
})
{
    Options = new CosmosDBCreateUpdateConfig(),
    Tags =
{
},
};
ArmOperation<CosmosDBSqlContainerResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, containerName, content);
CosmosDBSqlContainerResource result = lro.Value;

// the variable result is a resource, you could call other operations on this instance as well
// but just for demo, we get its data from this resource instance
CosmosDBSqlContainerData resourceData = result.Data;
// for demo we just print out the id
Console.WriteLine($"Succeeded on id: {resourceData.Id}");