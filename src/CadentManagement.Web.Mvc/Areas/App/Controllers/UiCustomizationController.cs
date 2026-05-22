using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.Configuration;
using CadentManagement.Web.Areas.App.Models.UiCustomization;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize]
public class UiCustomizationController : CadentManagementControllerBase
{
    private readonly IUiCustomizationSettingsAppService _uiCustomizationAppService;

    public UiCustomizationController(IUiCustomizationSettingsAppService uiCustomizationAppService)
    {
        _uiCustomizationAppService = uiCustomizationAppService;
    }

    public async Task<ActionResult> Index()
    {
        var model = new UiCustomizationViewModel
        {
            Theme = await SettingManager.GetSettingValueAsync(AppSettings.UiManagement.Theme),
            Settings = await _uiCustomizationAppService.GetUiManagementSettings(),
            HasUiCustomizationPagePermission = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Administration_UiCustomization)
        };

        return View(model);
    }
}

