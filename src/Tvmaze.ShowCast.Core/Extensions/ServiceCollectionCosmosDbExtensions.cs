using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Tvmaze.ShowCast.Core.Dal.DbContexts;
using Tvmaze.ShowCast.Core.Options;

namespace Tvmaze.ShowCast.Core.Extensions;

public static class ServiceCollectionCosmosDbExtensions
{
    public static IServiceCollection AddCosmosDbService(this IServiceCollection services,
        Action<CosmosDbOptions> cosmosDbOptionsBindAction)
    {
        var cloudmoreClientOptions = new CosmosDbOptions();
        cosmosDbOptionsBindAction(cloudmoreClientOptions);
        
        var cosmosClient = new CosmosClient(
            accountEndpoint: cloudmoreClientOptions.Endpoint,
            authKeyOrResourceToken: cloudmoreClientOptions.AccessKey
        );
        
        Database database = cosmosClient.CreateDatabaseIfNotExistsAsync(
            id: cloudmoreClientOptions.DatabaseName).GetAwaiter().GetResult();
        
        Container container = database.CreateContainerIfNotExistsAsync(
            id: cloudmoreClientOptions.ContainerName,
            partitionKeyPath: "/partitionKey",
            throughput: 400).GetAwaiter().GetResult();
        
        var cosmosDbService = new CosmosDbContext(container);
        
        services.AddSingleton<IDbContext>(cosmosDbService);
        
        return services;
    }
}