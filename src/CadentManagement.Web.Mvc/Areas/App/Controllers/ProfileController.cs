using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization.Users.Profile;
using CadentManagement.Configuration;
using CadentManagement.Timing;
using CadentManagement.Timing.Dto;
using CadentManagement.Web.Areas.App.Models.Profile;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize]
public class ProfileController : CadentManagementControllerBase
{
    private readonly IProfileAppService _profileAppService;
    private readonly ITimingAppService _timingAppService;
    private readonly ITenantCache _tenantCache;
    private readonly IMultiTenancyConfig _multiTenancyConfig;

    public ProfileController(
        IProfileAppService profileAppService,
        ITimingAppService timingAppService,
        ITenantCache tenantCache,
        IMultiTenancyConfig multiTenancyConfig)
    {
        _profileAppService = profileAppService;
        _timingAppService = timingAppService;
        _tenantCache = tenantCache;
        _multiTenancyConfig = multiTenancyConfig;
    }

    public async Task<PartialViewResult> MySettingsModal()
    {
        var output = await _profileAppService.GetCurrentUserProfileForEdit();
        var viewModel = ObjectMapper.Map<MySettingsViewModel>(output);
        viewModel.TimezoneItems = await _timingAppService.GetTimezoneComboboxItems(new GetTimezoneComboboxItemsInput
        {
            DefaultTimezoneScope = SettingScopes.User,
            SelectedTimezoneId = output.Timezone
        });
        viewModel.SmsVerificationEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.UserManagement.SmsVerificationEnabled);

        return PartialView("_MySettingsModal", viewModel);
    }

    public async Task<PartialViewResult> TwoFactorAuthenticationModal()
    {
        var result = await _profileAppService.GenerateGoogleAuthenticatorKey();

        return PartialView("_TwoFactorAuthentication", result);
    }

    public PartialViewResult ViewRecoveryCodesModal()
    {
        return PartialView("_ViewRecoveryCodesModal");
    }

    public PartialViewResult RemoveAuthenticatorModal()
    {
        return PartialView("_RemoveAuthenticatorModal");
    }

    public PartialViewResult ChangePictureModal(long? userId)
    {
        ViewBag.UserId = userId;
        return PartialView("_ChangePictureModal");
    }

    public PartialViewResult ChangePasswordModal()
    {
        return PartialView("_ChangePasswordModal");
    }

    public PartialViewResult SmsVerificationModal()
    {
        return PartialView("_SmsVerificationModal");
    }

    public PartialViewResult LinkedAccountsModal()
    {
        return PartialView("_LinkedAccountsModal");
    }

    public PartialViewResult LinkAccountModal()
    {
        ViewBag.TenancyName = GetTenancyNameOrNull();
        ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
        return PartialView("_LinkAccountModal");
    }

    public PartialViewResult UserDelegationsModal()
    {
        return PartialView("_UserDelegationsModal");
    }

    public PartialViewResult CreateNewUserDelegationModal()
    {
        return PartialView("_CreateNewUserDelegationModal");
    }

    private string GetTenancyNameOrNull()
    {
        if (!AbpSession.TenantId.HasValue)
        {
            return null;
        }

        return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
    }
}

