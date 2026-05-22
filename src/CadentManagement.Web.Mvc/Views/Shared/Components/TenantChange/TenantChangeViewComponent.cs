using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Session;
using CadentManagement.Sessions.Dto;

namespace CadentManagement.Web.Views.Shared.Components.TenantChange;

public class TenantChangeViewComponent : CadentManagementViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public TenantChangeViewComponent(IPerRequestSessionCache sessionCache)
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var loginInfo = await _sessionCache.GetCurrentLoginInformationsAsync();
        var model = new TenantChangeViewModel { Tenant = loginInfo.Tenant };
        return View(model);
    }
}

