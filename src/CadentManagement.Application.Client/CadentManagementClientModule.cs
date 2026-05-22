using Abp.Modules;
using Abp.Reflection.Extensions;

namespace CadentManagement;

public class CadentManagementClientModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(CadentManagementClientModule).GetAssembly());
    }
}

