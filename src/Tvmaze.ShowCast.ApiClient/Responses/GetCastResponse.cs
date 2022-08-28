using System.Text.Json.Serialization;

namespace Tvmaze.ShowCast.ApiClient.Responses;

public class GetCastResponse
{
    [JsonPropertyName("person")] 
    public Person Persons { get; set; }

    public class Person
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }
        
        [JsonPropertyName("name")] 
        public string Name { get; set; }
        
        [JsonPropertyName("birthday")] 
        public string? Birthday { get; set; }
    }
}