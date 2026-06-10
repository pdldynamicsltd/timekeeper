using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
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

        if (await IsGrantedAsync(AppPermissions.Pages_Tasks))
        {
            return Redirect(Url.Action("Index", "Tasks") + queryString);
        }

        if (await IsGrantedAsync(AppPermissions.Pages_TimeTracking))
        {
            return Redirect(Url.Action("Index", "TimeTracking") + queryString);
        }

        if (await IsGrantedAsync(AppPermissions.Pages_Tenants))
        {
            return Redirect(Url.Action("Index", "Tenants") + queryString);
        }

        return Redirect(Url.Action("Index", "Welcome") + queryString);
    }
}
