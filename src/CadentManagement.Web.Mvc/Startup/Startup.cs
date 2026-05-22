using System;
using System.IO;
using System.Linq;
using Abp.AspNetCore;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.AspNetZeroCore.Web.Authentication.JwtBearer;
using Abp.Castle.Logging.Log4Net;
using Abp.Hangfire;
using Abp.PlugIns;
using Castle.Facilities.Logging;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CadentManagement.Authorization;
using CadentManagement.Configuration;
using CadentManagement.Configure;
using CadentManagement.EntityFrameworkCore;
using CadentManagement.Identity;
using CadentManagement.Schemas;
using CadentManagement.Web.Chat.SignalR;
using CadentManagement.Web.Common;
using CadentManagement.Web.Resources;
using Swashbuckle.AspNetCore.Swagger;
using CadentManagement.Web.Swagger;
using Stripe;
using System.Reflection;
using System.Threading.RateLimiting;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Caching;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.HtmlSanitizer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using CadentManagement.Web.HealthCheck;
using Owl.reCAPTCHA;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CadentManagement.Web.Authentication.PasswordlessLogin;
using CadentManagement.Web.RateLimiting;
using CadentManagement.Web.Extensions;
using CadentManagement.Web.MultiTenancy;
using CadentManagement.Web.OpenIddict;
using SecurityStampValidatorCallback = CadentManagement.Identity.SecurityStampValidatorCallback;
using Abp.AspNetCore.OpenIddict;
using Abp.EntityFrameworkCore;
using Microsoft.OpenApi;
using CadentManagement.Authorization.QrLogin;

namespace CadentManagement.Web.Startup;

public class Startup
{
    private readonly IConfigurationRoot _appConfiguration;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public Startup(IWebHostEnvironment env)
    {
        _appConfiguration = env.GetAppConfiguration();
        _hostingEnvironment = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // MVC
        var mvcBuilder = services.AddControllersWithViews(options =>
        {
            // Anti-forgery validation can be disabled via appsettings.json for easier API testing
            // In production, set DisableAntiforgeryTokenValidation to false for security
            var disableAntiforgery = _appConfiguration.GetValue<bool>("App:DisableAntiforgeryTokenValidation");
            if (!disableAntiforgery)
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            }
            options.AddAbpHtmlSanitizer();
        });

#if DEBUG
        mvcBuilder.AddRazorRuntimeCompilation();
#endif

        if (bool.Parse(_appConfiguration["KestrelServer:IsEnabled"]))
        {
            ConfigureKestrel(services);
        }

        IdentityRegistrar.Register(services);

        if (bool.Parse(_appConfiguration["OpenIddict:IsEnabled"]))
        {
            OpenIddictRegistrar.Register(services, _appConfiguration);
        }

        services.Configure<SecurityStampValidatorOptions>(opts =>
        {
            opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
        });

        AuthConfigurer.Configure(services, _appConfiguration);

        if (WebConsts.SwaggerUiEnabled)
        {
            ConfigureSwagger(services);
        }

        services.AddDynamicRateLimiting();

        //Recaptcha
        services.AddreCAPTCHAV3(x =>
        {
            x.SiteKey = _appConfiguration["Recaptcha:SiteKey"];
            x.SiteSecret = _appConfiguration["Recaptcha:SecretKey"];
        });

        if (WebConsts.HangfireDashboardEnabled)
        {
            // Hangfire (Enable to use Hangfire instead of default job manager)
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(_appConfiguration.GetConnectionString("Default"));
            });

            services.AddHangfireServer();
        }

        services.AddScoped<IWebResourceManager, WebResourceManager>();

        services.AddSignalR();

        if (WebConsts.GraphQL.Enabled)
        {
            services.AddAndConfigureGraphQL();
        }

        services.Configure<SecurityStampValidatorOptions>(options => { options.ValidationInterval = TimeSpan.Zero; });

        ConfigureHealthChecks(services);
        
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new RazorViewLocationExpander());
        });

        //Configure Abp and Dependency Injection
        services.AddAbpWithoutCreatingServiceProvider<CadentManagementWebMvcModule>(options =>
        {
            //Configure Log4Net logging
            options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                f => f.UseAbpLog4Net().WithConfig(_hostingEnvironment.IsDevelopment()
                    ? "log4net.config"
                    : "log4net.Production.config")
            );

            options.PlugInSources.AddFolder(Path.Combine(_hostingEnvironment.WebRootPath, "Plugins"),
                SearchOption.AllDirectories);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseGetScriptsResponsePerUserCache();

        //Initializes ABP framework.
        app.UseAbp(options =>
        {
            options.UseAbpRequestLocalization = false; //used below: UseAbpRequestLocalization
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseCadentManagementForwardedHeaders();
        }
        else
        {
            app.UseStatusCodePagesWithRedirects("~/Error?statusCode={0}");
            app.UseExceptionHandler("/Error");
            app.UseCadentManagementForwardedHeaders();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

#pragma warning disable CS0162
        if (CadentManagementConsts.PreventNotExistingTenantSubdomains)
        {
            app.UseMiddleware<DomainTenantCheckMiddleware>();
        }

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();

        if (bool.Parse(_appConfiguration["Authentication:JwtBearer:IsEnabled"]))
        {
            app.UseJwtTokenMiddleware();
        }

        if (bool.Parse(_appConfiguration["OpenIddict:IsEnabled"]))
        {
            app.UseAbpOpenIddictValidation();
        }

        app.UseAuthorization();

        using (var scope = app.ApplicationServices.CreateScope())
        {
            if (scope.ServiceProvider.GetService<DatabaseCheckHelper>()
                .Exist(_appConfiguration["ConnectionStrings:Default"]))
            {
                app.UseAbpRequestLocalization();
            }
            else
            {
                app.Use(async (context, next) =>
                {
                    if (!context.Request.Path.StartsWithSegments("/Install"))
                    {
                        context.Response.Redirect("/Install");
                        return;
                    }

                    await next();
                });
            }
        }

        if (WebConsts.HangfireDashboardEnabled)
        {
            //Hangfire dashboard & server (Enable to use Hangfire instead of default job manager)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[]
                    {new AbpHangfireAuthorizationFilter(AppPermissions.Pages_Administration_HangfireDashboard)}
            });
        }

        if (bool.Parse(_appConfiguration["Payment:Stripe:IsActive"]))
        {
            StripeConfiguration.ApiKey = _appConfiguration["Payment:Stripe:SecretKey"];
        }

        if (WebConsts.GraphQL.Enabled)
        {
            app.UseGraphQL<MainSchema>(WebConsts.GraphQL.EndPoint);
            if (WebConsts.GraphQL.PlaygroundEnabled)
            {
                //to explorer API navigate https://*DOMAIN*/ui/playground
                app.UseGraphQLPlayground(
                    WebConsts.GraphQL.PlaygroundEndPoint,
                    new PlaygroundOptions()
                );
            }
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<AbpCommonHub>("/signalr");
            endpoints.MapHub<ChatHub>("/signalr-chat");
            endpoints.MapHub<QrLoginHub>("signalr-qr-login");

            endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>().EndpointConfiguration
                .ConfigureAllEndpoints(endpoints);
        });

        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
        });

        if (WebConsts.SwaggerUiEnabled)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
            //Enable middleware to serve swagger - ui assets(HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(_appConfiguration["App:SwaggerEndPoint"], "CadentManagement API V1");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("CadentManagement.Web.wwwroot.swagger.ui.index.html");
                options.InjectBaseUrl(_appConfiguration["App:WebSiteRootAddress"]);
            }); //URL: /swagger
        }
    }

    private void ConfigureKestrel(IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
        {
            options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 443),
                listenOptions =>
                {
                    var certPassword = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Password");
                    var certPath = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Path");
                    var cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(certPath,
                        certPassword);
                    listenOptions.UseHttps(new HttpsConnectionAdapterOptions()
                    {
                        ServerCertificate = cert
                    });
                });
        });
    }

    private void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo() { Title = "CadentManagement API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.ParameterFilter<SwaggerEnumParameterFilter>();
            options.SchemaFilter<SwaggerEnumSchemaFilter>();
            options.OperationFilter<SwaggerOperationIdFilter>();
            options.OperationFilter<SwaggerOperationFilter>();
            options.CustomDefaultSchemaIdSelector();

            // Add summaries to swagger
            var canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
            if (!canShowSummaries)
            {
                return;
            }

            var mvcXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var mvcXmlPath = Path.Combine(AppContext.BaseDirectory, mvcXmlFile);
            options.IncludeXmlComments(mvcXmlPath);

            var applicationXml = $"CadentManagement.Application.xml";
            var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
            options.IncludeXmlComments(applicationXmlPath);

            var webCoreXmlFile = $"CadentManagement.Web.Core.xml";
            var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
            options.IncludeXmlComments(webCoreXmlPath);
        });
    }

    private void ConfigureHealthChecks(IServiceCollection services)
    {
        services.AddAbpZeroHealthCheck();
    }
}