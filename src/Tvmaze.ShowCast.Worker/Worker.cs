using Microsoft.Extensions.Options;
using Tvmaze.ShowCast.Core.Bll.Services;
using Tvmaze.ShowCast.Worker.Options;

namespace Tvmaze.ShowCast.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly IShowCastService _showCastService;

    private readonly ScheduleOptions _scheduleOptions;

    public Worker(ILogger<Worker> logger, 
        IShowCastService showCastService, 
        IOptions<ScheduleOptions> scheduleOptions)
    {
        _logger = logger;
        _showCastService = showCastService;
        _scheduleOptions = scheduleOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                await _showCastService.CollectDataSequentially(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unpredicted exception");
            }
            
            await Task.Delay(_scheduleOptions.CollectDataTimeoutSeconds * 1000, stoppingToken);
        }
    }
}