using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.Editions;
using CadentManagement.MultiTenancy;
using CadentManagement.MultiTenancy.Payments;
using CadentManagement.Web.Areas.App.Models.Editions;
using CadentManagement.Web.Areas.App.Models.SubscriptionManagement;
using CadentManagement.Web.Controllers;
using CadentManagement.Web.Session;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement)]
public class SubscriptionManagementController : CadentManagementControllerBase
{
    private readonly IPaymentAppService _paymentAppService;
    private readonly IEditionAppService _editionAppService;
    private readonly ITenantRegistrationAppService _tenantRegistrationAppService;
    private readonly IPerRequestSessionCache _sessionCache;


    public SubscriptionManagementController(IPaymentAppService paymentAppService,
        IEditionAppService editionAppService,
        ITenantRegistrationAppService tenantRegistrationAppService,
        IPerRequestSessionCache sessionCache)
    {
        _paymentAppService = paymentAppService;
        _editionAppService = editionAppService;
        _sessionCache = sessionCache;
        _tenantRegistrationAppService = tenantRegistrationAppService;
    }

    public async Task<ActionResult> Index()
    {
        var loginInfo = await _sessionCache.GetCurrentLoginInformationsAsync();
        var editions = await _tenantRegistrationAppService.GetEditionsForSelect();
        var model = new SubscriptionDashboardViewModel
        {
            LoginInformations = loginInfo,
            Editions = editions
        };

        return View(model);
    }

    public async Task<PartialViewResult> ShowDetailModal(int id)
    {
        var payment = await _paymentAppService.GetPaymentAsync(id);

        var viewModel = ObjectMapper.Map<List<ShowDetailModalViewModel>>(payment.SubscriptionPaymentProducts);
        return PartialView("_ShowDetailModal", viewModel);
    }
}

