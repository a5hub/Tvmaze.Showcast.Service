using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace Tvmaze.ShowCast.ApiClient.ApiPolicies;

public class ApiPolicies : IApiPolicies
{
    private readonly ILogger<ApiPolicies> _logger;
        
    private readonly ApiPolicyOptions _options;

    public ApiPolicies(ILogger<ApiPolicies> logger, IOptions<ApiPolicyOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public IAsyncPolicy<HttpResponseMessage> WaitAndRetryAsync => Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(_options.MaxRetryCount,
            retryAttempt => TimeSpan.FromMilliseconds(_options.BackoffMultiplier * _options.RetryDelayMs * retryAttempt),
            async (response, delay, retryCount, context) =>
            {
                var content = response.Result?.Content;

                if (content != null)
                {
                    var contentStr = await content.ReadAsStringAsync().ConfigureAwait(false);

                    _logger.LogError(
                        "Status Code: {StatusCode}. Content: {Content}. Retry count exceeded {retryCount}",
                        response.Result?.StatusCode,
                        contentStr);

                    if (response.Exception != null)
                    {
                        _logger.LogError(response.Exception, "Retry count exceeded");
                    }
                }
                else
                {
                    switch (response.Exception)
                    {
                        case null:
                            throw new InvalidOperationException(
                                $"Fallback policy called with no exception, nor content, CorrelationId: {context?.CorrelationId}");
                            
                        case HttpRequestException requestException:
                            _logger.LogError(
                                requestException,
                                "Status Code: {StatusCode}: {Message}, retry count exceeded",
                                requestException.StatusCode,
                                requestException.Message);
                            break;
                            
                        default:
                            _logger.LogError(response.Exception, "Exception thrown in request processing");
                            break;
                    }

                    throw response.Exception;
                }
            })
        .WrapAsync(
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    _options.MaxRetryCount,
                    retryAttempt => TimeSpan.FromMilliseconds(_options.BackoffMultiplier * _options.RetryDelayMs * retryAttempt),
                    onRetry: async (
                        response,
                        delay,
                        retryCount,
                        context) =>
                    {
                        if (response.Exception != null)
                        {
                            _logger.LogError(
                                response.Exception,
                                "Delaying for {delay} ms, then making retry {retry} of {maxRetry}.",
                                delay.TotalMilliseconds,
                                retryCount,
                                _options.MaxRetryCount);
                        }
                        else
                        {
                            var content = response?.Result?.Content;
                            var contentStr = string.Empty;

                            if (content != null)
                            {
                                contentStr = await content.ReadAsStringAsync().ConfigureAwait(false);
                            }

                            _logger.LogError(
                                "Status Code: {statusCode}. Content: {content} Delaying for {delay}ms, then making retry {retry} of {maxRetry}.",
                                response?.Result?.StatusCode,
                                contentStr,
                                delay.TotalMilliseconds,
                                retryCount,
                                _options.MaxRetryCount);
                        }
                    })
        );
}