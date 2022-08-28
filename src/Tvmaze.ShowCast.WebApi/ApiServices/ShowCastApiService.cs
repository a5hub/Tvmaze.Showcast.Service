using Tvmaze.ShowCast.Core.Bll.Services;
using Tvmaze.ShowCast.WebApi.Cache;
using Tvmaze.ShowCast.WebApi.Models;
using Tvmaze.ShowCast.WebApi.Responses;

namespace Tvmaze.ShowCast.WebApi.ApiServices;

public class ShowCastApiService : IShowCastApiService
{
    private readonly IShowCastService _showCastService;

    private readonly ICache _cache;

    public ShowCastApiService(IShowCastService showCastService, 
        ICache cache)
    {
        _showCastService = showCastService;
        _cache = cache;
    }
    
    public async Task<IEnumerable<ShowCastResponse>> GetSortedDescPage(int page, CancellationToken token)
    {
        var result = await _cache.GetAsync<IEnumerable<ShowCastResponse>>(page.ToString(),token);
        if (result != null)
        {
            return result;
        }
        
        var data = await _showCastService.GetPageAsync(page, token);

        result = data.Select(x => new ShowCastResponse(x.Id, x.Name,
            x.Cast?.OrderByDescending(x => x.Birthday).Select(y => 
                new ShowCastCastModel(y.Id, y.Name, y.Birthday))));

        await _cache.SetAsync(page.ToString(), result, token);

        return result;
    }
}