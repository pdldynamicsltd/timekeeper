using Abp.AspNetZeroCore;
using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;
using CadentManagement.Configuration;
using CadentManagement.EntityFrameworkCore;
using CadentManagement.Migrator.DependencyInjection;

namespace CadentManagement.Migrator;

[DependsOn(typeof(CadentManagementEntityFrameworkCoreModule))]
public class CadentManagementMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public CadentManagementMigratorModule(CadentManagementEntityFrameworkCoreModule abpZeroTemplateEntityFrameworkCoreModule)
    {
        abpZeroTemplateEntityFrameworkCoreModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(CadentManagementMigratorModule).GetAssembly().GetDirectoryPathOrNull(),
            addUserSecrets: true
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            CadentManagementConsts.ConnectionStringName
            );
        Configuration.Modules.AspNetZero().LicenseCode = _appConfiguration["AbpZeroLicenseCode"];

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(typeof(IEventBus), () =>
        {
            IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            );
        });
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(CadentManagementMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}

