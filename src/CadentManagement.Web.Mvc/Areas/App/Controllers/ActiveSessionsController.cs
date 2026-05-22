using Abp.AspNetCore.Mvc.Authorization;
using Abp.Configuration;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.Configuration;
using CadentManagement.Web.Areas.App.Startup;
using CadentManagement.Web.Controllers;
using System.Threading.Tasks;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_ActiveSessions)]
public class ActiveSessionsController : CadentManagementControllerBase
{
    public async Task<ActionResult> Index()
    {
        var isSessionManagementEnabled = await SettingManager.GetSettingValueAsync<bool>(
            AppSettings.UserManagement.SessionManagement.IsEnabled);
        
        if (!isSessionManagementEnabled)
        {
            return NotFound();
        }

        var isSessionRevocationEnabled = await SettingManager.GetSettingValueAsync<bool>(
            AppSettings.UserManagement.SessionManagement.IsSessionRevocationEnabled);

        ViewBag.IsSessionRevocationEnabled = isSessionRevocationEnabled;
        ViewBag.CurrentPageName = AppPageNames.Common.ActiveSessions;
        return View();
    }
}
