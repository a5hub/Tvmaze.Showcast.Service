namespace Tvmaze.ShowCast.Core.Options;

public class CosmosDbOptions
{
    public const string Key = "CosmosDb";
    
    public string DatabaseName { get; set; }
    
    public string ContainerName { get; set; }
    
    public string Endpoint { get; set; }
    
    public string AccessKey { get; set; }
}