using Abp.Application.Services.Dto;
using Abp.Runtime.Validation;

namespace CadentManagement.RateLimiting.Dto;

public class GetRateLimitPoliciesInput : PagedAndSortedResultRequestDto, IShouldNormalize
{
    public string Filter { get; set; }

    public RateLimitAlgorithm? Algorithm { get; set; }

    public bool? IsEnabled { get; set; }

     public void Normalize()
    {
        if (string.IsNullOrEmpty(Sorting))
        {
            Sorting = "name ASC";
        }
    }
}
