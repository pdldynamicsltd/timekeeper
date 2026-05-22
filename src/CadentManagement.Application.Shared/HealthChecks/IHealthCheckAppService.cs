using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.HealthChecks.Dto;

namespace CadentManagement.HealthChecks;

public interface IHealthCheckAppService : IApplicationService
{
    Task<ListResultDto<HealthCheckItemDto>> GetHealthChecks();
}
