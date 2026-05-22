using CadentManagement.Maui.Services.Navigation;
using CadentManagement.Maui.Services.Permission;
using CadentManagement.Maui.Services.UI;

namespace CadentManagement.Maui.Core.Components;

public class CadentManagementMainLayoutPageComponentBase : CadentManagementComponentBase
{
    protected PageHeaderService PageHeaderService { get; set; }

    protected DomManipulatorService DomManipulatorService { get; set; }

    protected INavigationService NavigationService { get; set; }

    protected IPermissionService PermissionService { get; set; }

    public CadentManagementMainLayoutPageComponentBase()
    {
        PageHeaderService = Resolve<PageHeaderService>();
        DomManipulatorService = Resolve<DomManipulatorService>();
        NavigationService = Resolve<INavigationService>();
        PermissionService = Resolve<IPermissionService>();
    }

    protected async Task SetPageHeader(string title)
    {
        PageHeaderService.Title = title;
        PageHeaderService.SubTitle = string.Empty;
        PageHeaderService.ClearButton();
        await DomManipulatorService.ClearModalBackdrop(JS);
    }

    protected async Task SetPageHeader(string title, string subTitle)
    {
        PageHeaderService.Title = title;
        PageHeaderService.SubTitle = subTitle;
        PageHeaderService.ClearButton();
        await DomManipulatorService.ClearModalBackdrop(JS);
    }

    protected async Task SetPageHeader(string title, string subTitle, List<PageHeaderButton> buttons)
    {
        PageHeaderService.Title = title;
        PageHeaderService.SubTitle = subTitle;
        PageHeaderService.SetButtons(buttons);
        await DomManipulatorService.ClearModalBackdrop(JS);
    }
}