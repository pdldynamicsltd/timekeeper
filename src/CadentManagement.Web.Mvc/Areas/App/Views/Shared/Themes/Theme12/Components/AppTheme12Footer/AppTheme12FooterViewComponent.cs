using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Areas.App.Models.Layout;
using CadentManagement.Web.Session;
using CadentManagement.Web.Views;

namespace CadentManagement.Web.Areas.App.Views.Shared.Themes.Theme12.Components.AppTheme12Footer;

public class AppTheme12FooterViewComponent : CadentManagementViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public AppTheme12FooterViewComponent(IPerRequestSessionCache sessionCache)
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var footerModel = new FooterViewModel
        {
            LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync()
        };

        return View(footerModel);
    }
}

