using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.HealthChecks;
using CadentManagement.Web.Areas.App.Models.HealthCheckUI;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_HealthCheck)]
public class HealthCheckUIController : CadentManagementControllerBase
{
    private readonly IHealthCheckAppService _healthCheckAppService;

    public HealthCheckUIController(IHealthCheckAppService healthCheckAppService)
    {
        _healthCheckAppService = healthCheckAppService;
    }

    public async Task<ActionResult> Index()
    {
        var healthChecks = await _healthCheckAppService.GetHealthChecks();
        
        var model = new HealthCheckUIViewModel
        {
            HealthChecks = healthChecks.Items
        };

        return View(model);
    }
}
