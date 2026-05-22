using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CadentManagement.Configure;
using CadentManagement.Startup;
using CadentManagement.Test.Base;

namespace CadentManagement.GraphQL.Tests;

[DependsOn(
    typeof(CadentManagementGraphQLModule),
    typeof(CadentManagementTestBaseModule))]
public class CadentManagementGraphQLTestModule : AbpModule
{
    public override void PreInitialize()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddAndConfigureGraphQL();

        WindsorRegistrationHelper.CreateServiceProvider(IocManager.IocContainer, services);
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(CadentManagementGraphQLTestModule).GetAssembly());
    }
}