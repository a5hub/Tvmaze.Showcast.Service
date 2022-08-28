namespace Tvmaze.ShowCast.WebApi.Options;

public class CacheOptions
{
    public const string Key = "Cache";
    
    public string ConnectionString { get; set; }
    
    public int AbsoluteExpirationRelativeToNowMin { get; set; }
    
    public int SlidingExpirationMin { get; set; }
}