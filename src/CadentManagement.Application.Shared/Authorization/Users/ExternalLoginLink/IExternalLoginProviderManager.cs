using System.Collections.Generic;
using System.Threading.Tasks;
using CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

namespace CadentManagement.Authorization.Users.ExternalLoginLink;

public interface IExternalLoginProviderManager
{
    Task<string> GetProviderKeyAsync(string providerName, string accessCode);

    Task<ExternalLoginUserInfoDto> GetUserInfoAsync(string providerName, string accessCode);

    List<AvailableExternalLoginProvider> GetAvailableProviders();
}
