using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Tvmaze.ShowCast.WebApi.Converters;
using Tvmaze.ShowCast.WebApi.Options;

namespace Tvmaze.ShowCast.WebApi.Cache;

public class Cache : ICache
{
    private readonly IDistributedCache _distributedCache;

    private readonly JsonSerializerOptions _serializeOptions;
    
    private readonly CacheOptions _cacheOptions;
    
    public Cache(IDistributedCache distributedCache, 
        IOptions<CacheOptions> cacheOptions)
    {
        _distributedCache = distributedCache;
        _serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new DateOnlyConverter()
            }
        };
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken token)
    {
        var value = default(T);
        var cache = await _distributedCache.GetAsync(key, token);
        if ((cache?.Length ?? 0) > 0)
        {
            var cacheString = Encoding.UTF8.GetString(cache);
            value = JsonSerializer.Deserialize<T>(cacheString, _serializeOptions);
        }
        return value;
    }
    
    public async Task SetAsync<T>(string key, T value, CancellationToken token)
    {
        var valueSerialized = JsonSerializer.Serialize(value, _serializeOptions);
        var valueByteArray = Encoding.UTF8.GetBytes(valueSerialized);
        await _distributedCache.SetAsync(key, valueByteArray, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheOptions.AbsoluteExpirationRelativeToNowMin),
            SlidingExpiration = TimeSpan.FromMinutes(_cacheOptions.SlidingExpirationMin)
        }, token);
    }
}