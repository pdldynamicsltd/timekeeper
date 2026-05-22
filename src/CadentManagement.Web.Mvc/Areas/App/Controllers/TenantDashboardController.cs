using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.DashboardCustomization;
using System.Threading.Tasks;
using CadentManagement.Web.Areas.App.Startup;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Tenant_Dashboard)]
public class TenantDashboardController : CustomizableDashboardControllerBase
{
    public TenantDashboardController(DashboardViewConfiguration dashboardViewConfiguration,
        IDashboardCustomizationAppService dashboardCustomizationAppService)
        : base(dashboardViewConfiguration, dashboardCustomizationAppService)
    {

    }

    public async Task<ActionResult> Index()
    {
        return await GetView(CadentManagementDashboardCustomizationConsts.DashboardNames.DefaultTenantDashboard);
    }
}

