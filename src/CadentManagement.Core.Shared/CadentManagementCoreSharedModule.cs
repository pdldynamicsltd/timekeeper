using Abp.Modules;
using Abp.Reflection.Extensions;

namespace CadentManagement;

public class CadentManagementCoreSharedModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(CadentManagementCoreSharedModule).GetAssembly());
    }
}

