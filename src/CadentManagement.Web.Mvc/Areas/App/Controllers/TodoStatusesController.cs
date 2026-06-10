using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Tasks)]
public class TodoStatusesController : CadentManagementControllerBase
{
    public IActionResult Index()
    {
        return View();
    }
}
