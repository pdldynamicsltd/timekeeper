using Microsoft.Extensions.Configuration;

namespace CadentManagement.Configuration;

public interface IAppConfigurationAccessor
{
    IConfigurationRoot Configuration { get; }
}

