using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace CadentManagement.RateLimiting.Dto;

public class GetRateLimitPolicyForEditOutput
{
    public CreateOrEditRateLimitPolicyDto RateLimitPolicy { get; set; }

    public List<ComboboxItemDto> Algorithms { get; set; }

    public List<ComboboxItemDto> PartitionTypes { get; set; }
}
