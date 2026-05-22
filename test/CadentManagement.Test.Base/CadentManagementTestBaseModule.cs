using System;
using System.IO;
using Abp;
using Abp.AspNetZeroCore;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Castle.MicroKernel.Registration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using CadentManagement.Authorization.Users;
using CadentManagement.Configuration;
using CadentManagement.EntityFrameworkCore;
using CadentManagement.MultiTenancy;
using CadentManagement.Security.Recaptcha;
using CadentManagement.Test.Base.DependencyInjection;
using CadentManagement.Test.Base.UiCustomization;
using CadentManagement.Test.Base.Url;
using CadentManagement.Test.Base.Web;
using CadentManagement.UiCustomization;
using CadentManagement.Url;
using NSubstitute;

namespace CadentManagement.Test.Base;

[DependsOn(
    typeof(CadentManagementApplicationModule),
    typeof(CadentManagementEntityFrameworkCoreModule),
    typeof(AbpTestBaseModule))]
public class CadentManagementTestBaseModule : AbpModule
{
    public CadentManagementTestBaseModule(CadentManagementEntityFrameworkCoreModule abpZeroTemplateEntityFrameworkCoreModule)
    {
        abpZeroTemplateEntityFrameworkCoreModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        var configuration = GetConfiguration();

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

        Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
        Configuration.UnitOfWork.IsTransactional = false;

        //Use database for language management
        Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

        RegisterFakeService<AbpZeroDbMigrator>();
        RegisterFakeService<IHttpContextAccessor>();
        RegisterFakeService<RateLimiting.IRateLimitCacheInvalidator>();

        IocManager.Register<IAppUrlService, FakeAppUrlService>();
        IocManager.Register<IWebUrlService, FakeWebUrlService>();
        IocManager.Register<IRecaptchaValidator, FakeRecaptchaValidator>();

        Configuration.ReplaceService<IAppConfigurationAccessor, TestAppConfigurationAccessor>();
        Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
        Configuration.ReplaceService<IUiThemeCustomizerFactory, NullUiThemeCustomizerFactory>();

        Configuration.Modules.AspNetZero().LicenseCode = configuration["AbpZeroLicenseCode"];

        //Uncomment below line to write change logs for the entities below:
        Configuration.EntityHistory.IsEnabled = true;
        Configuration.EntityHistory.Selectors.Add("CadentManagementEntities", typeof(User), typeof(Tenant));
    }

    public override void Initialize()
    {
        ServiceCollectionRegistrar.Register(IocManager);
    }

    private void RegisterFakeService<TService>()
        where TService : class
    {
        IocManager.IocContainer.Register(
            Component.For<TService>()
                .UsingFactoryMethod(() => Substitute.For<TService>())
                .LifestyleSingleton()
        );
    }

    private static IConfigurationRoot GetConfiguration()
    {
        return AppConfigurations.Get(Directory.GetCurrentDirectory(), addUserSecrets: true);
    }
}
