using Serilog;
using Tvmaze.ShowCast.ApiClient.ApiPolicies;
using Tvmaze.ShowCast.ApiClient.Extensions;
using Tvmaze.ShowCast.ApiClient.Options;
using Tvmaze.ShowCast.Core.Bll.Services;
using Tvmaze.ShowCast.Core.Dal.Repository;
using Tvmaze.ShowCast.Core.Extensions;
using Tvmaze.ShowCast.Core.Options;
using Tvmaze.ShowCast.Worker;
using Tvmaze.ShowCast.Worker.Options;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Options
        services.Configure<TvmazeClientOptions>(context.Configuration.GetSection(TvmazeClientOptions.Key));
        services.Configure<ApiPolicyOptions>(context.Configuration.GetSection(ApiPolicyOptions.Key));
        services.Configure<ParallelismOptions>(context.Configuration.GetSection(ParallelismOptions.Key));
        services.Configure<ScheduleOptions>(context.Configuration.GetSection(ScheduleOptions.Key));
        
        // Workers
        services.AddHostedService<Worker>();
        
        // Api clients
        services.AddTransient<IApiPolicies, ApiPolicies>();
        services.AddTvmazeApiServices();
        
        // Services
        services.AddTransient<IShowCastService, ShowCastService>();
        services.AddTransient<IShowCastRepository, ShowCastRepository>();
        
        // Database
        services.AddCosmosDbService(context.Configuration.GetSection(CosmosDbOptions.Key).Bind);
        
        // Serilog
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(context.Configuration, "Serilog")
            .CreateLogger();
        Log.Logger = logger;
    })
    .UseSerilog()
    .Build();

await host.RunAsync();