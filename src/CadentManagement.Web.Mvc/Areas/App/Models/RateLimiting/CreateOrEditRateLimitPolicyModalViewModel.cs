using System.Collections.Generic;
using Abp.Application.Services.Dto;
using CadentManagement.RateLimiting.Dto;

namespace CadentManagement.Web.Areas.App.Models.RateLimiting;

public class CreateOrEditRateLimitPolicyModalViewModel
{
    public CreateOrEditRateLimitPolicyDto Policy { get; set; }

    public List<ComboboxItemDto> Algorithms { get; set; }

    public List<ComboboxItemDto> PartitionTypes { get; set; }

    public bool IsEditMode => Policy.Id.HasValue;
}
