using Microsoft.Azure.Cosmos;

namespace Tvmaze.ShowCast.Core.Dal.DbContexts;

public class CosmosDbContext : IDbContext
{
    private readonly Container _container;
    
    public CosmosDbContext(Container container)
    {
        _container = container;
    }

    public async Task UpsertAsync<T>(T newItem, CancellationToken token)
    {
        var t = newItem.GetType();
        var propertyInfo = t.GetProperty("PartitionKey");
        var partitionKey =  propertyInfo.GetValue(newItem).ToString();
        
        await _container.UpsertItemAsync(newItem, new PartitionKey(partitionKey), null, token);
    }

    public async Task<IList<T>> QueryAsync<T>(string queryText, CancellationToken token)
    {
        var queryResultSetIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(queryText));
        
        var result = new List<T>();
        
        while (queryResultSetIterator.HasMoreResults)
        {
            var currentResultSet = await queryResultSetIterator.ReadNextAsync(token);
            foreach (T family in currentResultSet)
            {
                result.Add(family);
            }
        }
        
        return result;
    }
}