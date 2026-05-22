using Microsoft.Extensions.DependencyInjection;
using CadentManagement.HealthChecks;

namespace CadentManagement.Web.HealthCheck;

public static class AbpZeroHealthCheck
{
    public static IHealthChecksBuilder AddAbpZeroHealthCheck(this IServiceCollection services)
    {
        var builder = services.AddHealthChecks();
        builder.AddCheck<CadentManagementDbContextHealthCheck>("Database Connection");
        builder.AddCheck<CadentManagementDbContextUsersHealthCheck>("Database Connection with user check");
        builder.AddCheck<CacheHealthCheck>("Cache");

        // add your custom health checks here
        // builder.AddCheck<MyCustomHealthCheck>("my health check");

        return builder;
    }
}

