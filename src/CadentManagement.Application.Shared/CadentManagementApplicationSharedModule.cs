using Abp.Modules;
using Abp.Reflection.Extensions;

namespace CadentManagement;

[DependsOn(typeof(CadentManagementCoreSharedModule))]
public class CadentManagementApplicationSharedModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(CadentManagementApplicationSharedModule).GetAssembly());
    }
}

