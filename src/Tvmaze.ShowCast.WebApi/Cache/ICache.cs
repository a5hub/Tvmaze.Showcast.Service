namespace Tvmaze.ShowCast.WebApi.Cache;

public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken token);

    Task SetAsync<T>(string key, T value, CancellationToken token);
}