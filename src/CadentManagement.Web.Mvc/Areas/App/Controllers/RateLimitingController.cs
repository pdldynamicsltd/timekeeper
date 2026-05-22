using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.RateLimiting;
using CadentManagement.Web.Areas.App.Models.RateLimiting;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_RateLimiting)]
public class RateLimitingController : CadentManagementControllerBase
{
    private readonly IRateLimitPolicyAppService _rateLimitPolicyAppService;

    public RateLimitingController(IRateLimitPolicyAppService rateLimitPolicyAppService)
    {
        _rateLimitPolicyAppService = rateLimitPolicyAppService;
    }

    public ActionResult Index()
    {
        return View();
    }

    [AbpMvcAuthorize(AppPermissions.Pages_Administration_RateLimiting_Create, AppPermissions.Pages_Administration_RateLimiting_Edit)]
    public async Task<PartialViewResult> CreateOrEditModal(int? id)
    {
        var output = await _rateLimitPolicyAppService.GetPolicyForEdit(new NullableIdDto { Id = id });

        var viewModel = new CreateOrEditRateLimitPolicyModalViewModel
        {
            Policy = output.RateLimitPolicy,
            Algorithms = output.Algorithms,
            PartitionTypes = output.PartitionTypes,
        };

        return PartialView("_CreateOrEditModal", viewModel);
    }
}
