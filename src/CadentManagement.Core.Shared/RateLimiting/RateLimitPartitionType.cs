namespace CadentManagement.RateLimiting;

public enum RateLimitPartitionType
{
    ByClientIp = 0,
    ByUser = 1,
    ByApiKey = 2
}
