using System;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Timing;
using Abp.UI;
using CadentManagement.Authorization.Users;

namespace CadentManagement.Authorization.Users.ExternalLoginLink;

[UnitOfWork]
public class ExternalLoginLinkManager : CadentManagementDomainServiceBase, IExternalLoginLinkManager, ITransientDependency
{
    private readonly ICacheManager _cacheManager;
    private readonly UserManager _userManager;
    private readonly IRepository<UserLogin, long> _userLoginRepository;
    private readonly IRepository<UserExternalLoginInfo, long> _userExternalLoginInfoRepository;

    public ExternalLoginLinkManager(
        ICacheManager cacheManager,
        UserManager userManager,
        IRepository<UserLogin, long> userLoginRepository,
        IRepository<UserExternalLoginInfo, long> userExternalLoginInfoRepository)
    {
        _cacheManager = cacheManager;
        _userManager = userManager;
        _userLoginRepository = userLoginRepository;
        _userExternalLoginInfoRepository = userExternalLoginInfoRepository;
    }

    public async Task<ExternalLoginLinkResult> LinkExternalLoginAsync(long userId, int? tenantId, string authProvider,
        string providerKey, string emailAddress)
    {
        var existingLogin = await _userLoginRepository.FirstOrDefaultAsync(ul =>
            ul.UserId == userId &&
            ul.LoginProvider == authProvider);

        if (existingLogin != null)
        {
            throw new UserFriendlyException(L("ExternalLoginAlreadyLinked"));
        }

        var otherUserLogin = await _userLoginRepository.FirstOrDefaultAsync(ul =>
            ul.LoginProvider == authProvider &&
            ul.ProviderKey == providerKey &&
            ul.TenantId == tenantId);

        if (otherUserLogin != null && otherUserLogin.UserId != userId)
        {
            var otherUser = await _userManager.FindByIdAsync(otherUserLogin.UserId.ToString());

            return new ExternalLoginLinkResult
            {
                Success = false,
                ProviderAlreadyLinkedToAnotherUser = true,
                CanMerge = true,
                ExistingUserEmail = MaskEmail(otherUser?.EmailAddress)
            };
        }

        await _userLoginRepository.InsertAsync(new UserLogin
        {
            LoginProvider = authProvider,
            ProviderKey = providerKey,
            TenantId = tenantId,
            UserId = userId
        });

        await UpsertExternalLoginEmailAsync(userId, tenantId, authProvider, emailAddress);

        await CurrentUnitOfWork.SaveChangesAsync();

        return new ExternalLoginLinkResult
        {
            Success = true
        };
    }

    public async Task MergeAndLinkExternalLoginAsync(long userId, int? tenantId, string authProvider,
        string providerKey, string emailAddress, string targetUserPassword)
    {
        var currentUserLogin = await _userLoginRepository.FirstOrDefaultAsync(ul =>
            ul.UserId == userId &&
            ul.LoginProvider == authProvider);

        if (currentUserLogin != null)
        {
            throw new UserFriendlyException(L("ExternalLoginAlreadyLinked"));
        }

        var existingLogin = await _userLoginRepository.FirstOrDefaultAsync(ul =>
            ul.LoginProvider == authProvider &&
            ul.ProviderKey == providerKey &&
            ul.TenantId == tenantId);

        if (existingLogin == null || existingLogin.UserId == userId)
        {
            throw new UserFriendlyException(L("ExternalLoginNotFound"));
        }

        var targetUser = await _userManager.FindByIdAsync(existingLogin.UserId.ToString());
        if (targetUser == null)
        {
            throw new UserFriendlyException(L("ExternalLoginNotFound"));
        }

        var passwordValid = await _userManager.CheckPasswordAsync(targetUser, targetUserPassword);
        if (!passwordValid)
        {
            throw new UserFriendlyException(L("InvalidPassword"));
        }

        var existingEmailAddress = await GetExternalLoginEmailAddressAsync(existingLogin.UserId, existingLogin.TenantId,
            authProvider);
        var emailAddressToStore = string.IsNullOrWhiteSpace(emailAddress) ? existingEmailAddress : emailAddress.Trim();

        await _userLoginRepository.DeleteAsync(existingLogin);
        await DeleteExternalLoginEmailAsync(existingLogin.UserId, existingLogin.TenantId, authProvider);
        await CurrentUnitOfWork.SaveChangesAsync();

        await _userLoginRepository.InsertAsync(new UserLogin
        {
            LoginProvider = authProvider,
            ProviderKey = providerKey,
            TenantId = tenantId,
            UserId = userId
        });

        await UpsertExternalLoginEmailAsync(userId, tenantId, authProvider, emailAddressToStore);

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public async Task<string> CreateMergeTokenAsync(long userId, int? tenantId, string authProvider, string providerKey,
        string emailAddress)
    {
        var token = Guid.NewGuid().ToString("N");

        await _cacheManager.GetExternalLoginLinkMergeCache().SetAsync(token, new ExternalLoginLinkMergeCacheItem
        {
            UserId = userId,
            TenantId = tenantId,
            AuthProvider = authProvider,
            ProviderKey = providerKey,
            EmailAddress = string.IsNullOrWhiteSpace(emailAddress) ? null : emailAddress.Trim()
        }, ExternalLoginLinkMergeCacheItem.DefaultSlidingExpireTime);

        return token;
    }

    public async Task MergeAndLinkExternalLoginAsync(long userId, int? tenantId, string mergeToken,
        string targetUserPassword)
    {
        var cacheItem = await _cacheManager.GetExternalLoginLinkMergeCache().GetOrDefaultAsync(mergeToken);
        if (cacheItem == null || cacheItem.UserId != userId || cacheItem.TenantId != tenantId)
        {
            throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
        }

        await MergeAndLinkExternalLoginAsync(userId, tenantId, cacheItem.AuthProvider, cacheItem.ProviderKey,
            cacheItem.EmailAddress,
            targetUserPassword);

        await _cacheManager.GetExternalLoginLinkMergeCache().RemoveAsync(mergeToken);
    }

    private async Task UpsertExternalLoginEmailAsync(long userId, int? tenantId, string authProvider, string emailAddress)
    {
        var normalizedEmailAddress = string.IsNullOrWhiteSpace(emailAddress) ? null : emailAddress.Trim();
        if (normalizedEmailAddress == null)
        {
            return;
        }

        var existingInfo = await GetExternalLoginInfoAsync(userId, tenantId, authProvider);
        if (existingInfo == null)
        {
            await _userExternalLoginInfoRepository.InsertAsync(new UserExternalLoginInfo
            {
                UserId = userId,
                TenantId = tenantId,
                LoginProvider = authProvider,
                EmailAddress = normalizedEmailAddress,
                CreationTime = Clock.Now
            });

            return;
        }

        existingInfo.EmailAddress = normalizedEmailAddress;
    }

    private async Task DeleteExternalLoginEmailAsync(long userId, int? tenantId, string authProvider)
    {
        var existingInfo = await GetExternalLoginInfoAsync(userId, tenantId, authProvider);
        if (existingInfo != null)
        {
            await _userExternalLoginInfoRepository.DeleteAsync(existingInfo);
        }
    }

    private async Task<string> GetExternalLoginEmailAddressAsync(long userId, int? tenantId, string authProvider)
    {
        return (await GetExternalLoginInfoAsync(userId, tenantId, authProvider))?.EmailAddress;
    }

    private Task<UserExternalLoginInfo> GetExternalLoginInfoAsync(long userId, int? tenantId, string authProvider)
    {
        return _userExternalLoginInfoRepository.FirstOrDefaultAsync(info =>
            info.UserId == userId &&
            info.TenantId == tenantId &&
            info.LoginProvider == authProvider);
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return "***";
        }

        var atIndex = email.IndexOf('@');
        if (atIndex < 0)
        {
            return "***";
        }

        if (atIndex <= 1)
        {
            return "***" + email[atIndex..];
        }

        return email[0] + new string('*', atIndex - 1) + email[atIndex..];
    }
}
