using CadentManagement.ApiClient;
using CadentManagement.ApiClient.Models;
using CadentManagement.Sessions.Dto;

namespace CadentManagement.Maui.Services.Storage;

public interface IDataStorageService
{
    Task StoreAccessTokenAsync(string newAccessToken, string newEncryptedAccessToken);

    Task StoreAuthenticateResultAsync(AbpAuthenticateResultModel authenticateResultModel);

    AbpAuthenticateResultModel RetrieveAuthenticateResult();

    TenantInformation RetrieveTenantInfo();

    GetCurrentLoginInformationsOutput RetrieveLoginInfo();

    void ClearSessionPersistance();

    Task StoreLoginInformationAsync(GetCurrentLoginInformationsOutput loginInfo);

    Task StoreTenantInfoAsync(TenantInformation tenantInfo);
}