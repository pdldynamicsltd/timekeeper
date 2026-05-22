using Flurl.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CadentManagement.ApiClient;
using CadentManagement.Maui.Core;
using CadentManagement.Maui.Core.Helpers;
using CadentManagement.Maui.Core.Localization;
using CadentManagement.Maui.Services.Account;
using CadentManagement.Maui.Services.Navigation;
using CadentManagement.Maui.Services.Tenants;
using CadentManagement.Maui.Services.UI;

#if ANDROID
using CadentManagement.Maui.Core.Platforms.Android.HttpClient;
#endif
#if IOS
using CadentManagement.Maui.Core.Platforms.iOS.HttpClient;
#endif

namespace CadentManagement.Maui.Pages.Layout;

public partial class MainLayout
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    protected IJSRuntime JS { get; set; }

    protected UserDialogsService UserDialogsService { get; set; }

    private bool IsConfigurationsInitialized { get; set; }

    private string _logoURL;

    protected override async Task OnInitializedAsync()
    {
        UserDialogsService = DependencyResolver.Resolve<UserDialogsService>();
        UserDialogsService.Initialize(JS);

        await UserDialogsService.Block();

        await CheckInternetAndStartApplication();

        var navigationService = DependencyResolver.Resolve<INavigationService>();
        navigationService.Initialize(NavigationManager);

        _logoURL = await DependencyResolver.Resolve<TenantCustomizationService>().GetTenantLogo();

        await SetLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(200);
            await JS.InvokeVoidAsync("KTMenu.init");
        }
    }

    private async Task CheckInternetAndStartApplication()
    {
        if (Connectivity.NetworkAccess == NetworkAccess.Internet || ApiUrlConfig.IsLocal)
        {
            await StartApplication();
        }
        else
        {
            var isTryAgain = await UserDialogsService.Instance.Confirm(L.Localize("NoInternet"));
            if (!isTryAgain)
            {
                CurrentApplicationCloser.Quit();
            }

            await CheckInternetAndStartApplication();
        }
    }

    private async Task StartApplication()
    {
        ConfigureFlurlHttp();
        App.LoadPersistedSession();

        if (UserConfigurationManager.HasConfiguration)
        {
            IsConfigurationsInitialized = true;
            await UserDialogsService.UnBlock();

        }
        else
        {
            await UserConfigurationManager.GetAsync(async () =>
            {
                IsConfigurationsInitialized = true;
                await UserDialogsService.UnBlock();
            });
        }
    }

    private static void ConfigureFlurlHttp()
    {
        FlurlHttp.Clients.WithDefaults(builer => builer
            .OnError(call => HandleHttpErrorAsync(call))
            .ConfigureInnerHandler(hch =>
            {
#if ANDROID
                new AndroidHttpClientHandlerConfigurer().Configure(hch);
#elif IOS
                new IOSHttpClientHandlerConfigurer().Configure(hch);
#endif
            }));
    }

    private static async Task HandleHttpErrorAsync(FlurlCall call)
    {
        await new FlurlHttpErrorHandler().Handle(call);
    }

    private async Task SetLayout()
    {
        var dom = DependencyResolver.Resolve<DomManipulatorService>();
        await dom.ClearAllAttributes(JS, "body");
        await dom.SetAttribute(JS, "body", "id", "kt_app_body");
        await dom.SetAttribute(JS, "body", "data-kt-app-layout", "light-sidebar");
        await dom.SetAttribute(JS, "body", "data-kt-app-sidebar-enabled", "true");
        await dom.SetAttribute(JS, "body", "data-kt-app-sidebar-fixed", "true");
        await dom.SetAttribute(JS, "body", "data-kt-app-toolbar-enabled", "true");
        await dom.SetAttribute(JS, "body", "class", "app-default");
    }
}