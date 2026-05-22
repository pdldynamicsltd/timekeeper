using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Session;

namespace CadentManagement.Web.Views.Shared.Components.AccountLogo;

public class AccountLogoViewComponent : CadentManagementViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public AccountLogoViewComponent(IPerRequestSessionCache sessionCache)
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync(string skin)
    {
        var loginInfo = await _sessionCache.GetCurrentLoginInformationsAsync();
        return View(new AccountLogoViewModel(loginInfo, skin));
    }
}

