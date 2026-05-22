using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Areas.App.Models.Layout;
using CadentManagement.Web.Session;
using CadentManagement.Web.Views;

namespace CadentManagement.Web.Areas.App.Views.Shared.Components.AppLogo;

public class AppLogoViewComponent : CadentManagementViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public AppLogoViewComponent(
        IPerRequestSessionCache sessionCache
    )
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync(string logoSkin = null, string logoClass = "")
    {
        var headerModel = new LogoViewModel
        {
            LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync(),
            LogoSkinOverride = logoSkin,
            LogoClassOverride = logoClass
        };

        return View(headerModel);
    }
}

