using Abp.Dependency;
using Abp.Runtime.Caching;
using CadentManagement.RateLimiting;

namespace CadentManagement.Web.RateLimiting;

public class RateLimitCacheInvalidator : IRateLimitCacheInvalidator, ITransientDependency
{
    private const string CacheName = "AppRateLimitPolicies";

    private readonly ICacheManager _cacheManager;

    public RateLimitCacheInvalidator(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public void InvalidateCache()
    {
        _cacheManager.GetCache(CacheName).Clear();
    }
}
