using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize]
public class WelcomeController : CadentManagementControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}

