using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Areas.App.Models.Layout;
using CadentManagement.Web.Session;
using CadentManagement.Web.Views;

namespace CadentManagement.Web.Areas.App.Views.Shared.Themes.Theme12.Components.AppTheme12Brand;

public class AppTheme12BrandViewComponent : CadentManagementViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public AppTheme12BrandViewComponent(IPerRequestSessionCache sessionCache)
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var headerModel = new HeaderViewModel
        {
            LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync()
        };

        return View(headerModel);
    }
}

