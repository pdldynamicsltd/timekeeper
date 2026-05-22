using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Web.Areas.App.Models.Layout;
using CadentManagement.Web.Views;

namespace CadentManagement.Web.Areas.App.Views.Shared.Components.AppChatToggler;

public class AppChatTogglerViewComponent : CadentManagementViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(string cssClass, string iconClass = "flaticon-chat-2 fs-4")
    {
        return Task.FromResult<IViewComponentResult>(View(new ChatTogglerViewModel
        {
            CssClass = cssClass,
            IconClass = iconClass
        }));
    }
}

