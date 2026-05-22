using System;
using System.Threading.Tasks;
using CadentManagement.ApiClient.Models;

namespace CadentManagement.ApiClient;

public interface IAccessTokenManager
{
    string GetAccessToken();

    string GetEncryptedAccessToken();

    Task<AbpAuthenticateResultModel> LoginAsync();

    Task<(string accessToken, string encryptedAccessToken)?> RefreshTokenAsync();

    void Logout();

    bool IsUserLoggedIn { get; }

    bool IsRefreshTokenExpired { get; }

    AbpAuthenticateResultModel AuthenticateResult { get; set; }

    DateTime AccessTokenRetrieveTime { get; set; }
}

