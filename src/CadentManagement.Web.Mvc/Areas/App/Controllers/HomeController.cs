using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize]
public class HomeController : CadentManagementControllerBase
{
    public async Task<ActionResult> Index()
    {
        var queryString = Request.QueryString.Value ?? string.Empty;

        if (AbpSession.MultiTenancySide == MultiTenancySides.Host)
        {
            if (await IsGrantedAsync(AppPermissions.Pages_Administration_Host_Dashboard))
            {
                return Redirect(Url.Action("Index", "HostDashboard") + queryString);
            }

            if (await IsGrantedAsync(AppPermissions.Pages_Tenants))
            {
                return Redirect(Url.Action("Index", "Tenants") + queryString);
            }
        }
        else
        {
            if (await IsGrantedAsync(AppPermissions.Pages_Tenant_Dashboard))
            {
                return Redirect(Url.Action("Index", "TenantDashboard") + queryString);
            }
        }

        return Redirect(Url.Action("Index", "Welcome") + queryString);
    }
}

