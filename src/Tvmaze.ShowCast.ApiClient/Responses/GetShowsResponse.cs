using System.Text.Json.Serialization;

namespace Tvmaze.ShowCast.ApiClient.Responses;

public class GetShowsResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
}