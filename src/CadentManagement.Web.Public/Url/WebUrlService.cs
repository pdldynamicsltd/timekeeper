using Abp.Dependency;
using CadentManagement.Configuration;
using CadentManagement.Url;
using CadentManagement.Web.Url;

namespace CadentManagement.Web.Public.Url;

public class WebUrlService : WebUrlServiceBase, IWebUrlService, ITransientDependency
{
    public WebUrlService(
        IAppConfigurationAccessor appConfigurationAccessor) :
        base(appConfigurationAccessor)
    {
    }

    public override string WebSiteRootAddressFormatKey => "App:WebSiteRootAddress";

    public override string ServerRootAddressFormatKey => "App:AdminWebSiteRootAddress";
}

