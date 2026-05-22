using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetZeroCore.Web.Authentication.External;
using Abp.Dependency;
using CadentManagement.Authorization.Users.ExternalLoginLink;
using CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

namespace CadentManagement.Web.Authentication.External;

public class ExternalLoginProviderManager : IExternalLoginProviderManager, ITransientDependency
{
    private readonly IExternalAuthConfiguration _externalAuthConfiguration;
    private readonly IExternalAuthManager _externalAuthManager;

    public ExternalLoginProviderManager(
        IExternalAuthConfiguration externalAuthConfiguration,
        IExternalAuthManager externalAuthManager)
    {
        _externalAuthConfiguration = externalAuthConfiguration;
        _externalAuthManager = externalAuthManager;
    }

    public async Task<string> GetProviderKeyAsync(string providerName, string accessCode)
    {
        return (await GetUserInfoAsync(providerName, accessCode)).ProviderKey;
    }

    public async Task<ExternalLoginUserInfoDto> GetUserInfoAsync(string providerName, string accessCode)
    {
        var userInfo = await _externalAuthManager.GetUserInfo(providerName, accessCode);
        return new ExternalLoginUserInfoDto
        {
            ProviderKey = userInfo.ProviderKey,
            EmailAddress = userInfo.EmailAddress
        };
    }

    public List<AvailableExternalLoginProvider> GetAvailableProviders()
    {
        return _externalAuthConfiguration.ExternalLoginInfoProviders
            .Select(p => p.GetExternalLoginInfo())
            .Select(p => new AvailableExternalLoginProvider
            {
                Name = p.Name,
                ClientId = p.ClientId,
                AdditionalParams = p.AdditionalParams
            })
            .ToList();
    }
}
