using Abp.Modules;
using Abp.Reflection.Extensions;

namespace CadentManagement.Startup;

[DependsOn(typeof(CadentManagementCoreModule))]
public class CadentManagementGraphQLModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(CadentManagementGraphQLModule).GetAssembly());
    }
}

