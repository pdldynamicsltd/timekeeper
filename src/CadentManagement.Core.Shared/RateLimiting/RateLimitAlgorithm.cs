namespace CadentManagement.RateLimiting;

public enum RateLimitAlgorithm
{
    FixedWindow = 0,
    SlidingWindow = 1,
    TokenBucket = 2,
    Concurrency = 3
}
