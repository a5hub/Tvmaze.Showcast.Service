using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using Tvmaze.ShowCast.ApiClient.ApiPolicies;
using Tvmaze.ShowCast.ApiClient.Clients;
using Tvmaze.ShowCast.ApiClient.Options;

namespace Tvmaze.ShowCast.ApiClient.Extensions;

public static class ServiceCollectionTvmazeApiClientExtensions
{
    public static IServiceCollection AddTvmazeApiServices(this IServiceCollection services)
    {
        services
            .AddRefitClient<ITvmazeWebapi>()
            .ConfigureHttpClient(
                (sp, c) =>
                {
                    c.BaseAddress = new Uri(sp.GetRequiredService<IOptions<TvmazeClientOptions>>().Value.TvmazeApiUrl);
                })
            .AddPolicyHandler((sp, req) => sp.GetRequiredService<IApiPolicies>().WaitAndRetryAsync);

        return services;
    }
}