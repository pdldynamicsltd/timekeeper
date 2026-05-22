using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using CadentManagement.EntityFrameworkCore;

namespace CadentManagement.HealthChecks;

public class CadentManagementDbContextHealthCheck : IHealthCheck
{
    private readonly DatabaseCheckHelper _checkHelper;

    public CadentManagementDbContextHealthCheck(DatabaseCheckHelper checkHelper)
    {
        _checkHelper = checkHelper;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        if (_checkHelper.Exist("db"))
        {
            return Task.FromResult(HealthCheckResult.Healthy("CadentManagementDbContext connected to database."));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("CadentManagementDbContext could not connect to database"));
    }
}
