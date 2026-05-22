using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.RateLimiting.Dto;

namespace CadentManagement.RateLimiting;

public interface IRateLimitPolicyAppService : IApplicationService
{
    Task<bool> GetIsEnabled();

    Task SetIsEnabled(bool isEnabled);

    Task<PagedResultDto<RateLimitPolicyDto>> GetPolicies(GetRateLimitPoliciesInput input);

    Task<GetRateLimitPolicyForEditOutput> GetPolicyForEdit(NullableIdDto input);

    Task CreateOrEdit(CreateOrEditRateLimitPolicyDto input);

    Task Delete(EntityDto input);

    Task TogglePolicyEnabled(EntityDto input);
}
