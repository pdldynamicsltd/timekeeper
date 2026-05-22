using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Public.Controllers;

public class HomeController : CadentManagementControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}

