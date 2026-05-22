using System.Threading.Tasks;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Web.MultiTenancy;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using CadentManagement.Web.MultiTenancy;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CadentManagement.Tests.MultiTenancy;

public class DomainTenantCheckMiddleware_Tests
{
    private const string DomainFormat = "https://{TENANCY_NAME}.yourdomain.com/";

    private readonly IWebMultiTenancyConfiguration _multiTenancyConfig;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantStore _tenantStore;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    private readonly DomainTenantCheckMiddleware _middleware;

    private bool _nextCalled;

    public DomainTenantCheckMiddleware_Tests()
    {
        _multiTenancyConfig = Substitute.For<IWebMultiTenancyConfiguration>();
        _multiTenancyConfig.DomainFormat.Returns(DomainFormat);

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _tenantStore = Substitute.For<ITenantStore>();

        _unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        var fakeUow = Substitute.For<IUnitOfWorkCompleteHandle>();
        _unitOfWorkManager.Begin(Arg.Any<UnitOfWorkOptions>()).Returns(fakeUow);

        _middleware = new DomainTenantCheckMiddleware(
            _multiTenancyConfig,
            _httpContextAccessor,
            _tenantStore,
            _unitOfWorkManager)
        {
            Logger = NullLogger.Instance
        };

        _nextCalled = false;
    }

    private DefaultHttpContext CreateHttpContext(string host, string path = "/")
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);
        context.Request.Scheme = "https";
        context.Request.Path = path;
        _httpContextAccessor.HttpContext.Returns(context);
        return context;
    }

    private async Task<DefaultHttpContext> InvokeMiddleware(string host, string path = "/")
    {
        var context = CreateHttpContext(host, path);
        _nextCalled = false;

        await _middleware.InvokeAsync(context, ctx =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        });

        return context;
    }

    [Fact]
    public async Task Should_Pass_Through_For_Host_Domain()
    {
        await InvokeMiddleware("yourdomain.com");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_For_Host_Domain_Case_Insensitive()
    {
        await InvokeMiddleware("YourDomain.COM");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_For_Www_Subdomain()
    {
        await InvokeMiddleware("www.yourdomain.com");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_For_Www_Case_Insensitive()
    {
        await InvokeMiddleware("WWW.yourdomain.com");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_For_Existing_Tenant()
    {
        _tenantStore.Find("acme").Returns(new TenantInfo(1, "acme"));

        await InvokeMiddleware("acme.yourdomain.com");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Redirect_For_Non_Existing_Tenant()
    {
        _tenantStore.Find("nonexistent").Returns((TenantInfo)null);

        var context = await InvokeMiddleware("nonexistent.yourdomain.com");

        _nextCalled.ShouldBeFalse();
        context.Response.Headers["Location"].ToString()
            .ShouldContain("yourdomain.com");
        context.Response.Headers["Location"].ToString()
            .ShouldContain("Error?statusCode=404");
    }

    [Fact]
    public async Task Should_Redirect_To_Host_Url_Not_Tenant_Url()
    {
        _tenantStore.Find("badtenant").Returns((TenantInfo)null);

        var context = await InvokeMiddleware("badtenant.yourdomain.com");

        var location = context.Response.Headers["Location"].ToString();
        location.ShouldNotContain("badtenant.");
        location.ShouldStartWith("https://yourdomain.com/");
    }

    [Fact]
    public async Task Should_Pass_Through_For_Error_Path()
    {
        await InvokeMiddleware("nonexistent.yourdomain.com", "/Error");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_For_Error_Path_With_Query()
    {
        var context = CreateHttpContext("nonexistent.yourdomain.com", "/Error");
        context.Request.QueryString = new QueryString("?statusCode=404");
        _nextCalled = false;

        await _middleware.InvokeAsync(context, ctx =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        });

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_When_DomainFormat_Is_Empty()
    {
        _multiTenancyConfig.DomainFormat.Returns(string.Empty);

        await InvokeMiddleware("anything.yourdomain.com");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_When_DomainFormat_Is_Null()
    {
        _multiTenancyConfig.DomainFormat.Returns((string)null);

        await InvokeMiddleware("anything.yourdomain.com");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_When_HttpContext_Is_Null()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext)null);

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("nonexistent.yourdomain.com");
        context.Request.Scheme = "https";
        _nextCalled = false;

        await _middleware.InvokeAsync(context, ctx =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        });

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_For_Host_Domain_With_Port()
    {
        _multiTenancyConfig.DomainFormat.Returns("https://{TENANCY_NAME}.localhost:44302/");

        await InvokeMiddleware("localhost");

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Redirect_For_Non_Existing_Tenant_With_Port()
    {
        _multiTenancyConfig.DomainFormat.Returns("https://{TENANCY_NAME}.localhost:44302/");
        _tenantStore.Find("badtenant").Returns((TenantInfo)null);

        var context = await InvokeMiddleware("badtenant.localhost");

        _nextCalled.ShouldBeFalse();
        var location = context.Response.Headers["Location"].ToString();
        location.ShouldContain("Error?statusCode=404");
    }

    [Fact]
    public async Task Should_Pass_Through_For_Host_Domain_With_Multiple_Formats()
    {
        _multiTenancyConfig.DomainFormat.Returns(
            "https://{TENANCY_NAME}.app.yourdomain.com/;https://{TENANCY_NAME}.api.yourdomain.com/"
        );

        await InvokeMiddleware("app.yourdomain.com");
        _nextCalled.ShouldBeTrue();

        await InvokeMiddleware("api.yourdomain.com");
        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Pass_Through_For_Unrelated_Domain()
    {
        await InvokeMiddleware("someotherdomain.com");

        _nextCalled.ShouldBeTrue();
    }
}
