using System.Threading.Tasks;
using Abp.Configuration.Startup;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.Web.Areas.App.Models.Layout;
using CadentManagement.Web.Session;
using CadentManagement.Web.Views;

namespace CadentManagement.Web.Areas.App.Views.Shared.Components.AppUserMenu;

public class AppUserMenuViewComponent : CadentManagementViewComponent
{
    private readonly IMultiTenancyConfig _multiTenancyConfig;
    private readonly IAbpSession _abpSession;
    private readonly IPerRequestSessionCache _sessionCache;

    public AppUserMenuViewComponent(
        IPerRequestSessionCache sessionCache,
        IMultiTenancyConfig multiTenancyConfig,
        IAbpSession abpSession)
    {
        _sessionCache = sessionCache;
        _multiTenancyConfig = multiTenancyConfig;
        _abpSession = abpSession;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        string togglerCssClass,
        string textCssClass,
        string symbolCssClass,
        string symbolTextCssClas,
        string anchorCssClass,
        bool renderOnlyIcon = false,
        string profileImageCssClass = "")
    {
        return View(new UserMenuViewModel
        {
            LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync(),
            IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
            IsImpersonatedLogin = _abpSession.ImpersonatorUserId.HasValue,
            HasUiCustomizationPagePermission = await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_Administration_UiCustomization),
            TogglerCssClass = togglerCssClass,
            TextCssClass = textCssClass,
            SymbolCssClass = symbolCssClass,
            SymbolTextCssClass = symbolTextCssClas,
            AnchorCssClass = anchorCssClass,
            RenderOnlyIcon = renderOnlyIcon,
            ProfileImageCssClass = profileImageCssClass
        });
    }
}

