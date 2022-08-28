using Tvmaze.ShowCast.Core.Bll.Services;
using Tvmaze.ShowCast.WebApi.Models;
using Tvmaze.ShowCast.WebApi.Responses;

namespace Tvmaze.ShowCast.WebApi.ApiServices;

public class ShowCastApiService : IShowCastApiService
{
    private readonly IShowCastService _showCastService;

    public ShowCastApiService(IShowCastService showCastService)
    {
        _showCastService = showCastService;
    }
    
    public async Task<IEnumerable<ShowCastResponse>> GetSortedDescPage(int page, CancellationToken token)
    {
        var data = await _showCastService.GetPageAsync(page, token);

        return data.Select(x => new ShowCastResponse(x.Id, x.Name,
            x.Cast?.OrderByDescending(x => x.Birthday).Select(y => 
                new ShowCastCastModel(y.Id, y.Name, y.Birthday))));
    }
}