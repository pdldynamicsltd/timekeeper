using Abp.Modules;
using Abp.Reflection.Extensions;
using CadentManagement.Authorization;

namespace CadentManagement;

/// <summary>
/// Application layer module of the application.
/// </summary>
[DependsOn(
    typeof(CadentManagementApplicationSharedModule),
    typeof(CadentManagementCoreModule)
    )]
public class CadentManagementApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        //Adding authorization providers
        Configuration.Authorization.Providers.Add<AppAuthorizationProvider>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(CadentManagementApplicationModule).GetAssembly());
    }
}
