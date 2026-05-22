using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.Text;
using Abp.Web.MultiTenancy;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using CadentManagement.Web.Url;

namespace CadentManagement.Web.MultiTenancy;

public class DomainTenantCheckMiddleware : IMiddleware, ITransientDependency
{
    private readonly IWebMultiTenancyConfiguration _multiTenancyConfiguration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantStore _tenantStore;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ILogger Logger { get; set; }

    public DomainTenantCheckMiddleware(
        IWebMultiTenancyConfiguration multiTenancyConfiguration,
        IHttpContextAccessor httpContextAccessor,
        ITenantStore tenantStore,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _multiTenancyConfiguration = multiTenancyConfiguration;
        _httpContextAccessor = httpContextAccessor;
        _tenantStore = tenantStore;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_multiTenancyConfiguration.DomainFormat.IsNullOrEmpty())
        {
            await next(context);
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/Error"))
        {
            await next(context);
            return;
        }

        var hostName = httpContext.Request.Host.Host.RemovePreFix("http://", "https://").RemovePostFix("/");
        var domainFormats = _multiTenancyConfiguration.DomainFormat
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(df => df.Trim())
            .ToArray();

        if (IsHostDomain(domainFormats, hostName))
        {
            await next(context);
            return;
        }

        var result = IsDomainFormatValid(domainFormats, hostName);

        if (result.extraction?.Matches == null)
        {
            await next(context);
            return;
        }

        var tenancyName = result.extraction.Matches[0].Value;
        if (tenancyName.IsNullOrEmpty())
        {
            await next(context);
            return;
        }

        if (string.Equals(tenancyName, "www", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var tenantInfo = _unitOfWorkManager.WithUnitOfWork(() => _tenantStore.Find(tenancyName));

        if (tenantInfo == null)
        {
            Logger.Warn($"There is no such tenant: {tenancyName}");
            var hostUrl = GetHostUrl(result.matchedFormat, httpContext);
            context.Response.Redirect(hostUrl.EnsureEndsWith('/') + "Error?statusCode=" +
                                      (int)HttpStatusCode.NotFound);
        }
        else
        {
            await next(context);
        }
    }

    private bool IsHostDomain(string[] domainFormats, string hostName)
    {
        foreach (var item in domainFormats)
        {
            var domainFormat = item
                .RemovePreFix("http://", "https://")
                .Split(':')[0]
                .RemovePostFix("/");

            var hostDomain = RemoveTenancyPlaceholder(domainFormat);

            if (string.Equals(hostName, hostDomain, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private string GetHostUrl(string matchedDomainFormat, HttpContext httpContext)
    {
        var domainFormat = matchedDomainFormat
            .RemovePreFix("http://", "https://")
            .RemovePostFix("/");

        var hostDomain = RemoveTenancyPlaceholder(domainFormat);

        if (!hostDomain.IsNullOrEmpty())
        {
            return httpContext.Request.Scheme + "://" + hostDomain;
        }

        return httpContext.Request.Scheme + "://" + httpContext.Request.Host.Value;
    }

    private static string RemoveTenancyPlaceholder(string domainFormat)
    {
        return domainFormat
            .Replace(WebUrlServiceBase.TenancyNamePlaceHolder + ".", "")
            .Replace(WebUrlServiceBase.TenancyNamePlaceHolder, "");
    }

    private (FormattedStringValueExtracter.ExtractionResult extraction, string matchedFormat) IsDomainFormatValid(
        string[] domainFormats, string hostName)
    {
        foreach (var item in domainFormats)
        {
            var domainFormat = item.RemovePreFix("http://", "https://").Split(':')[0].RemovePostFix("/");
            var result = new FormattedStringValueExtracter().Extract(hostName, domainFormat, true, '/');

            if (result.IsMatch && result.Matches.Any())
            {
                return (result, item);
            }
        }

        return (null, null);
    }
}

