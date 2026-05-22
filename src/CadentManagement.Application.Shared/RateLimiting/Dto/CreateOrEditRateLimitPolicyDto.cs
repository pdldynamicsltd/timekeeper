using System.ComponentModel.DataAnnotations;

namespace CadentManagement.RateLimiting.Dto;

public class CreateOrEditRateLimitPolicyDto
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(RateLimitPolicyConsts.MaxNameLength)]
    public string Name { get; set; }

    public bool IsEnabled { get; set; }

    public RateLimitAlgorithm Algorithm { get; set; }

    public RateLimitPartitionType PartitionType { get; set; }

    public bool IsGlobal { get; set; }

    [MaxLength(RateLimitPolicyConsts.MaxEndpointPatternLength)]
    public string EndpointPattern { get; set; }

    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; }

    [Range(0, int.MaxValue)]
    public int WindowInSeconds { get; set; }

    [Range(0, int.MaxValue)]
    public int QueueLimit { get; set; }

    [Range(0, int.MaxValue)]
    public int SegmentsPerWindow { get; set; }

    [Range(0, int.MaxValue)]
    public int TokensPerPeriod { get; set; }

    [Range(0, int.MaxValue)]
    public int ReplenishmentPeriodInSeconds { get; set; }

    [Range(100, 599)]
    public int HttpStatusCode { get; set; } = 429;

    [MaxLength(RateLimitPolicyConsts.MaxCustomMessageLength)]
    public string CustomMessage { get; set; }
}
