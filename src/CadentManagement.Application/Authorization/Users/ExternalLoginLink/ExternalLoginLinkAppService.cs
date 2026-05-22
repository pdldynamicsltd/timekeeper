using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization.Users;
using CadentManagement.Authorization.Users.ExternalLoginLink.Dto;
using CadentManagement.Configuration;

namespace CadentManagement.Authorization.Users.ExternalLoginLink;

[AbpAuthorize]
public class ExternalLoginLinkAppService : CadentManagementAppServiceBase, IExternalLoginLinkAppService
{
    private readonly IExternalLoginLinkManager _externalLoginLinkManager;
    private readonly IRepository<UserLogin, long> _userLoginRepository;
    private readonly IRepository<UserExternalLoginInfo, long> _userExternalLoginInfoRepository;
    private readonly IExternalLoginProviderManager _externalLoginProviderManager;
    private readonly ISettingManager _settingManager;

    private static readonly Dictionary<string, string> ProviderDeactivationSettings = new()
    {
        [ExternalLoginProviderNames.OpenIdConnect] = AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated,
        [ExternalLoginProviderNames.Microsoft] = AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated,
        [ExternalLoginProviderNames.Google] = AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated,
        [ExternalLoginProviderNames.Twitter] = AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated,
        [ExternalLoginProviderNames.Facebook] = AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated,
        [ExternalLoginProviderNames.WsFederation] = AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated,
    };

    public ExternalLoginLinkAppService(
        IExternalLoginLinkManager externalLoginLinkManager,
        IRepository<UserLogin, long> userLoginRepository,
        IRepository<UserExternalLoginInfo, long> userExternalLoginInfoRepository,
        IExternalLoginProviderManager externalLoginProviderManager,
        ISettingManager settingManager)
    {
        _externalLoginLinkManager = externalLoginLinkManager;
        _userLoginRepository = userLoginRepository;
        _userExternalLoginInfoRepository = userExternalLoginInfoRepository;
        _externalLoginProviderManager = externalLoginProviderManager;
        _settingManager = settingManager;
    }

    public async Task<List<ExternalLoginProviderDto>> GetExternalLogins()
    {
        var userId = AbpSession.GetUserId();

        var allProviders = _externalLoginProviderManager.GetAvailableProviders()
            .Where(IsProviderEnabledOnTenant)
            .ToList();

        var userLogins = await _userLoginRepository.GetAllListAsync(ul => ul.UserId == userId);
        var linkedProviders = userLogins.Select(ul => ul.LoginProvider).ToHashSet();
        var externalLoginInfos = await _userExternalLoginInfoRepository.GetAllListAsync(info => info.UserId == userId);
        var linkedProviderEmails = externalLoginInfos
            .GroupBy(info => info.LoginProvider)
            .ToDictionary(group => group.Key, group => group.First().EmailAddress);
        var linkedCount = userLogins.Count;

        var user = await UserManager.GetUserByIdAsync(userId);
        var isExternalLoginOnly = await UserManager.IsExternalLoginOnlyAsync(user);
        var canUnlink = linkedCount > 1 || !isExternalLoginOnly;

        return allProviders.Select(p =>
        {
            var isLinked = linkedProviders.Contains(p.Name);
            linkedProviderEmails.TryGetValue(p.Name, out var emailAddress);

            return new ExternalLoginProviderDto
            {
                Name = p.Name,
                ClientId = p.ClientId,
                IsLinked = isLinked,
                CanUnlink = isLinked && canUnlink,
                EmailAddress = isLinked ? emailAddress : null,
                AdditionalParams = p.AdditionalParams
            };
        }).ToList();
    }

    [HttpPost]
    public async Task<LinkExternalLoginResult> LinkExternalLogin(LinkExternalLoginInput input)
    {
        var externalUserInfo = await ValidateExternalLoginUserAsync(
            input.AuthProvider,
            input.ProviderKey,
            input.ProviderAccessCode);

        var result = await _externalLoginLinkManager.LinkExternalLoginAsync(
            AbpSession.GetUserId(),
            AbpSession.TenantId,
            input.AuthProvider,
            externalUserInfo.ProviderKey,
            externalUserInfo.EmailAddress);

        return new LinkExternalLoginResult
        {
            Success = result.Success,
            ProviderAlreadyLinkedToAnotherUser = result.ProviderAlreadyLinkedToAnotherUser,
            CanMerge = result.CanMerge,
            ExistingUserEmail = result.ExistingUserEmail
        };
    }

    [HttpPost]
    public async Task<UnlinkExternalLoginResult> UnlinkExternalLogin(UnlinkExternalLoginInput input)
    {
        var userId = AbpSession.GetUserId();

        var userLogin = await _userLoginRepository.FirstOrDefaultAsync(ul =>
            ul.UserId == userId &&
            ul.LoginProvider == input.AuthProvider);

        if (userLogin == null)
        {
            throw new UserFriendlyException(L("ExternalLoginNotFound"));
        }

        var otherExternalLogins = await _userLoginRepository.CountAsync(ul =>
            ul.UserId == userId &&
            ul.LoginProvider != input.AuthProvider);

        if (otherExternalLogins == 0)
        {
            var user = await UserManager.GetUserByIdAsync(userId);
            if (await UserManager.IsExternalLoginOnlyAsync(user))
            {
                return new UnlinkExternalLoginResult
                {
                    Success = false,
                    RequiresPasswordSetup = true
                };
            }
        }

        await _userLoginRepository.DeleteAsync(userLogin);
        var externalLoginInfo = await _userExternalLoginInfoRepository.FirstOrDefaultAsync(info =>
            info.UserId == userId &&
            info.TenantId == AbpSession.TenantId &&
            info.LoginProvider == input.AuthProvider);

        if (externalLoginInfo != null)
        {
            await _userExternalLoginInfoRepository.DeleteAsync(externalLoginInfo);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return new UnlinkExternalLoginResult
        {
            Success = true
        };
    }

    [HttpPost]
    public async Task MergeAndLinkExternalLogin(MergeExternalLoginInput input)
    {
        var externalUserInfo = await ValidateExternalLoginUserAsync(
            input.AuthProvider,
            input.ProviderKey,
            input.ProviderAccessCode);

        await _externalLoginLinkManager.MergeAndLinkExternalLoginAsync(
            AbpSession.GetUserId(),
            AbpSession.TenantId,
            input.AuthProvider,
            externalUserInfo.ProviderKey,
            externalUserInfo.EmailAddress,
            input.TargetUserPassword);
    }

    private bool IsProviderEnabledOnTenant(AvailableExternalLoginProvider provider)
    {
        if (!AbpSession.TenantId.HasValue)
        {
            return true;
        }

        if (!ProviderDeactivationSettings.TryGetValue(provider.Name, out var settingKey))
        {
            return true;
        }

        return !_settingManager.GetSettingValueForTenant<bool>(settingKey, AbpSession.GetTenantId());
    }

    private async Task<ExternalLoginUserInfoDto> ValidateExternalLoginUserAsync(string authProvider, string providerKey,
        string providerAccessCode)
    {
        var userInfo = await _externalLoginProviderManager.GetUserInfoAsync(authProvider, providerAccessCode);
        if (!ExternalLoginProviderKeyComparer.AreEqual(providerKey, userInfo.ProviderKey))
        {
            throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
        }

        return userInfo;
    }
}
