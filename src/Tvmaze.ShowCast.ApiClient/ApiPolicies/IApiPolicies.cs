using Polly;

namespace Tvmaze.ShowCast.ApiClient.ApiPolicies
{
    public interface IApiPolicies
    {
        IAsyncPolicy<HttpResponseMessage> WaitAndRetryAsync { get; }
    }
}
