using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tvmaze.ShowCast.ApiClient.Clients;
using Tvmaze.ShowCast.Core.Bll.Models;
using Tvmaze.ShowCast.Core.Dal.Dtos;
using Tvmaze.ShowCast.Core.Dal.Repository;
using Tvmaze.ShowCast.Core.Extensions;
using Tvmaze.ShowCast.Core.Options;

namespace Tvmaze.ShowCast.Core.Bll.Services;

public class ShowCastService : IShowCastService
{
    private readonly IShowCastRepository _showCastRepository;

    private readonly ITvmazeWebapi _tvmazeWebapi;

    private readonly ILogger<ShowCastService> _logger;
    
    private readonly ParallelismOptions _parallelismOptions;
    
    private const int TvmazePageSizeConst = 250;
    
    public ShowCastService(IShowCastRepository showCastRepository, 
        ITvmazeWebapi tvmazeWebapi, 
        ILogger<ShowCastService> logger,
        IOptions<ParallelismOptions> parallelismOptions)
    {
        _showCastRepository = showCastRepository;
        _tvmazeWebapi = tvmazeWebapi;
        _logger = logger;
        _parallelismOptions = parallelismOptions.Value;
    }
    
    public async Task CollectDataSequentially(CancellationToken token)
    {
        var lastSyncedPageNum = await GetLastSyncedPageNumAsync(token);
        await ScrapData(lastSyncedPageNum, token);
    }
    
    public async Task<IEnumerable<ShowCastModel>> GetPageAsync(int pageNumber, CancellationToken token)
    {
        var data = await _showCastRepository.GetItemsPageAsync(pageNumber, token);

        return data.Select(x => new ShowCastModel(x.Id, x.Name,
            x.Cast?.Select(y => new CastModel(y.Id, y.Name, y.BirthDay))));
    }
    
    private async Task<int> GetLastSyncedPageNumAsync(CancellationToken token)
    {
        var lastRecordId = await _showCastRepository.GetLastRecordIdAsync(token);
        var lastSyncedPage = lastRecordId / TvmazePageSizeConst;
        return lastSyncedPage;
    }

    private async Task ScrapData(int pageNumber, CancellationToken token)
    {
        var timer = new Stopwatch();
        timer.Start();
        
        _logger.LogInformation("Start scraping from page {0}", pageNumber);
        
        while (true)
        {
            var shows = await _tvmazeWebapi.GetShowsAsync(pageNumber, token);
            if (!shows.Any()) break;
            
            await Parallel.ForEachAsync(shows,
                new ParallelOptions { MaxDegreeOfParallelism = _parallelismOptions.TvmazeApiMaxDegreeOfParallelism },
                async (show, _) =>
                {
                    var cast = await _tvmazeWebapi.GetCastAsync(show.Id, token);
                    
                    var showCast = new ShowCastDalDto(show.Id, show.Name, cast.Select(x => 
                        new CastDalDto(x.Persons.Id, x.Persons.Name, x.Persons.Birthday?.ToDateOnly())));
                
                    await _showCastRepository.UpsertItemAsync(showCast, pageNumber, token);
                });
            
            _logger.LogInformation("Page {0} with {1} records scraped for {2} sec", 
                pageNumber,  shows.Count(), timer.ElapsedMilliseconds / 1000);
            
            pageNumber++;
            timer.Restart();
        }
    }
}