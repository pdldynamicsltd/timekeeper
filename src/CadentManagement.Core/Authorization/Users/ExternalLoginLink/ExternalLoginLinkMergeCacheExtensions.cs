using Abp.Runtime.Caching;

namespace CadentManagement.Authorization.Users.ExternalLoginLink;

public static class ExternalLoginLinkMergeCacheExtensions
{
    public static ITypedCache<string, ExternalLoginLinkMergeCacheItem> GetExternalLoginLinkMergeCache(
        this ICacheManager cacheManager)
    {
        return cacheManager.GetCache<string, ExternalLoginLinkMergeCacheItem>(ExternalLoginLinkMergeCacheItem.CacheName);
    }
}
