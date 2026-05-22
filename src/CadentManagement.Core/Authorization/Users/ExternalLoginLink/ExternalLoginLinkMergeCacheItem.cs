using System;

namespace CadentManagement.Authorization.Users.ExternalLoginLink;

[Serializable]
public class ExternalLoginLinkMergeCacheItem
{
    public const string CacheName = "AppExternalLoginLinkMergeCache";

    public static readonly TimeSpan DefaultSlidingExpireTime = TimeSpan.FromMinutes(5);

    public int? TenantId { get; set; }

    public long UserId { get; set; }

    public string AuthProvider { get; set; }

    public string ProviderKey { get; set; }

    public string EmailAddress { get; set; }
}
