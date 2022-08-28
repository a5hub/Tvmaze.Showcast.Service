using Tvmaze.ShowCast.WebApi.Responses;

namespace Tvmaze.ShowCast.WebApi.ApiServices;

public interface IShowCastApiService
{
    Task<IEnumerable<ShowCastResponse>> GetSortedDescPage(int page, CancellationToken token);
}