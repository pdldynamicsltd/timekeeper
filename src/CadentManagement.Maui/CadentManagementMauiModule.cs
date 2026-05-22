using Abp.Mapperly;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CadentManagement.ApiClient;
using CadentManagement.Maui.Core;

namespace CadentManagement.Maui;

[DependsOn(typeof(CadentManagementClientModule), typeof(AbpMapperlyModule))]
public class CadentManagementMauiModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Localization.IsEnabled = false;
        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

        Configuration.ReplaceService<IApplicationContext, MauiApplicationContext>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(CadentManagementMauiModule).GetAssembly());
    }
}