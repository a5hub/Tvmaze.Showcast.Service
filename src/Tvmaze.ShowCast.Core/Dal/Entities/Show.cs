using Newtonsoft.Json;

namespace Tvmaze.ShowCast.Core.Dal.Entities;

public class ShowCastEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Key { get; set; }
    
    [JsonProperty(PropertyName = "partitionKey")]
    public string PartitionKey { get; set; }
    
    public int Id { get; set; }

    public string Name { get; set; }
    
    public IEnumerable<Cast>? Cast { get; set; }
}