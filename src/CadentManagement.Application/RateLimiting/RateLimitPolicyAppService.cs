using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CadentManagement.Authorization;
using CadentManagement.Configuration;
using CadentManagement.RateLimiting.Dto;

namespace CadentManagement.RateLimiting;

[AbpAuthorize(AppPermissions.Pages_Administration_RateLimiting)]
public class RateLimitPolicyAppService : CadentManagementAppServiceBase, IRateLimitPolicyAppService
{
    private readonly IRepository<RateLimitPolicy> _rateLimitPolicyRepository;
    private readonly IRateLimitCacheInvalidator _cacheInvalidator;

    public RateLimitPolicyAppService(
        IRepository<RateLimitPolicy> rateLimitPolicyRepository,
        IRateLimitCacheInvalidator cacheInvalidator)
    {
        _rateLimitPolicyRepository = rateLimitPolicyRepository;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<bool> GetIsEnabled()
    {
        return await SettingManager.GetSettingValueForApplicationAsync<bool>(AppSettings.RateLimiting.IsEnabled);
    }

    public async Task SetIsEnabled(bool isEnabled)
    {
        await SettingManager.ChangeSettingForApplicationAsync(AppSettings.RateLimiting.IsEnabled, isEnabled.ToString().ToLowerInvariant());
        _cacheInvalidator.InvalidateCache();
    }

    [HttpPost]
    public async Task<PagedResultDto<RateLimitPolicyDto>> GetPolicies(GetRateLimitPoliciesInput input)
    {
        var query = _rateLimitPolicyRepository.GetAll()
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), p => p.Name.Contains(input.Filter))
            .WhereIf(input.Algorithm.HasValue, p => p.Algorithm == input.Algorithm.Value)
            .WhereIf(input.IsEnabled.HasValue, p => p.IsEnabled == input.IsEnabled.Value);

        var totalCount = await query.CountAsync();

        var policies = await query
            .OrderBy(input.Sorting)
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<RateLimitPolicyDto>(
            totalCount,
            ObjectMapper.Map<List<RateLimitPolicyDto>>(policies)
        );
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_RateLimiting_Create, AppPermissions.Pages_Administration_RateLimiting_Edit)]
    public async Task<GetRateLimitPolicyForEditOutput> GetPolicyForEdit(NullableIdDto input)
    {
        CreateOrEditRateLimitPolicyDto policyDto;

        if (input.Id.HasValue)
        {
            var policy = await _rateLimitPolicyRepository.GetAsync(input.Id.Value);
            policyDto = ObjectMapper.Map<CreateOrEditRateLimitPolicyDto>(policy);
        }
        else
        {
            policyDto = new CreateOrEditRateLimitPolicyDto
            {
                IsEnabled = true,
                Algorithm = RateLimitAlgorithm.FixedWindow,
                PartitionType = RateLimitPartitionType.ByClientIp,
                IsGlobal = true,
                PermitLimit = 100,
                WindowInSeconds = 60,
                QueueLimit = 0,
                SegmentsPerWindow = 1,
                TokensPerPeriod = 10,
                ReplenishmentPeriodInSeconds = 1,
                HttpStatusCode = 429,
            };
        }

        return new GetRateLimitPolicyForEditOutput
        {
            RateLimitPolicy = policyDto,
            Algorithms = GetAlgorithmComboboxItems(policyDto.Algorithm),
            PartitionTypes = GetPartitionTypeComboboxItems(policyDto.PartitionType),
        };
    }

    private static List<ComboboxItemDto> GetAlgorithmComboboxItems(RateLimitAlgorithm selectedAlgorithm)
    {
        return
        [
            new ComboboxItemDto(((int)RateLimitAlgorithm.FixedWindow).ToString(), nameof(RateLimitAlgorithm.FixedWindow)) { IsSelected = selectedAlgorithm == RateLimitAlgorithm.FixedWindow },
            new ComboboxItemDto(((int)RateLimitAlgorithm.SlidingWindow).ToString(), nameof(RateLimitAlgorithm.SlidingWindow)) { IsSelected = selectedAlgorithm == RateLimitAlgorithm.SlidingWindow },
            new ComboboxItemDto(((int)RateLimitAlgorithm.TokenBucket).ToString(), nameof(RateLimitAlgorithm.TokenBucket)) { IsSelected = selectedAlgorithm == RateLimitAlgorithm.TokenBucket },
            new ComboboxItemDto(((int)RateLimitAlgorithm.Concurrency).ToString(), nameof(RateLimitAlgorithm.Concurrency)) { IsSelected = selectedAlgorithm == RateLimitAlgorithm.Concurrency },
        ];
    }

    private static List<ComboboxItemDto> GetPartitionTypeComboboxItems(RateLimitPartitionType selectedPartitionType)
    {
        return
        [
            new ComboboxItemDto(((int)RateLimitPartitionType.ByClientIp).ToString(), nameof(RateLimitPartitionType.ByClientIp)) { IsSelected = selectedPartitionType == RateLimitPartitionType.ByClientIp },
            new ComboboxItemDto(((int)RateLimitPartitionType.ByUser).ToString(), nameof(RateLimitPartitionType.ByUser)) { IsSelected = selectedPartitionType == RateLimitPartitionType.ByUser },
            new ComboboxItemDto(((int)RateLimitPartitionType.ByApiKey).ToString(), nameof(RateLimitPartitionType.ByApiKey)) { IsSelected = selectedPartitionType == RateLimitPartitionType.ByApiKey },
        ];
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_RateLimiting_Create, AppPermissions.Pages_Administration_RateLimiting_Edit)]
    public async Task CreateOrEdit(CreateOrEditRateLimitPolicyDto input)
    {
        if (input.Id.HasValue)
        {
            await Update(input);
        }
        else
        {
            await Create(input);
        }
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_RateLimiting_Delete)]
    public async Task Delete(EntityDto input)
    {
        await _rateLimitPolicyRepository.DeleteAsync(input.Id);
        _cacheInvalidator.InvalidateCache();
    }

    public async Task TogglePolicyEnabled(EntityDto input)
    {
        var policy = await _rateLimitPolicyRepository.GetAsync(input.Id);
        policy.IsEnabled = !policy.IsEnabled;
        _cacheInvalidator.InvalidateCache();
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_RateLimiting_Create)]
    private async Task Create(CreateOrEditRateLimitPolicyDto input)
    {
        var policy = ObjectMapper.Map<RateLimitPolicy>(input);
        await _rateLimitPolicyRepository.InsertAsync(policy);
        _cacheInvalidator.InvalidateCache();
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_RateLimiting_Edit)]
    private async Task Update(CreateOrEditRateLimitPolicyDto input)
    {
        var policy = await _rateLimitPolicyRepository.GetAsync(input.Id.Value);

        if (policy == null)
        {
            throw new UserFriendlyException(L("RateLimitPolicyNotFound"));
        }

        ObjectMapper.Map(input, policy);
        _cacheInvalidator.InvalidateCache();
    }
}
