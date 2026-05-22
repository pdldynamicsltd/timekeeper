using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Areas.App.Models.Layout;
using CadentManagement.Web.Views;

namespace CadentManagement.Web.Areas.App.Views.Shared.Components.AppToggleDarkMode;

public class AppToggleDarkModeViewComponent : CadentManagementViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(string cssClass, bool isDarkModeActive)
    {
        return Task.FromResult<IViewComponentResult>(View(new ToggleDarkModeViewModel(cssClass, isDarkModeActive)));
    }
}

