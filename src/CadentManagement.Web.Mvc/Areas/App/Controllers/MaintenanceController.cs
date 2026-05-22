using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.Caching;
using CadentManagement.Web.Areas.App.Models.Maintenance;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_Maintenance)]
public class MaintenanceController : CadentManagementControllerBase
{
    private readonly ICachingAppService _cachingAppService;

    public MaintenanceController(ICachingAppService cachingAppService)
    {
        _cachingAppService = cachingAppService;
    }

    public ActionResult Index()
    {
        var model = new MaintenanceViewModel
        {
            Caches = _cachingAppService.GetAllCaches().Items,
            CanClearAllCaches = _cachingAppService.CanClearAllCaches()
        };

        return View(model);
    }
}

