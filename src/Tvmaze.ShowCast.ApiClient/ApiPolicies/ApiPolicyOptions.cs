namespace Tvmaze.ShowCast.ApiClient.ApiPolicies;

public record ApiPolicyOptions
{
    public const string Key = "ApiPolicy";
        
    public int MaxRetryCount { get; init; }
        
    public int RetryDelayMs { get; init; }

    public double BackoffMultiplier { get; init; }
}