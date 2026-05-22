using System.Threading.Tasks;

namespace CadentManagement.Authorization.Users.ExternalLoginLink;

public interface IExternalLoginLinkManager
{
    Task<ExternalLoginLinkResult> LinkExternalLoginAsync(long userId, int? tenantId, string authProvider,
        string providerKey, string emailAddress);

    Task MergeAndLinkExternalLoginAsync(long userId, int? tenantId, string authProvider, string providerKey,
        string emailAddress, string targetUserPassword);

    Task<string> CreateMergeTokenAsync(long userId, int? tenantId, string authProvider, string providerKey,
        string emailAddress);

    Task MergeAndLinkExternalLoginAsync(long userId, int? tenantId, string mergeToken, string targetUserPassword);
}
