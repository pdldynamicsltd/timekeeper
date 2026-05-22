using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace CadentManagement.RateLimiting;

[Table("AppRateLimitPolicies")]
public class RateLimitPolicy : FullAuditedEntity
{
    [Required]
    [MaxLength(RateLimitPolicyConsts.MaxNameLength)]
    public string Name { get; set; }

    public bool IsEnabled { get; set; }

    public RateLimitAlgorithm Algorithm { get; set; }

    public RateLimitPartitionType PartitionType { get; set; }

    public bool IsGlobal { get; set; }

    [MaxLength(RateLimitPolicyConsts.MaxEndpointPatternLength)]
    public string EndpointPattern { get; set; }

    public int PermitLimit { get; set; }

    public int WindowInSeconds { get; set; }

    public int QueueLimit { get; set; }

    // Sliding Window specific
    public int SegmentsPerWindow { get; set; }

    // Token Bucket specific
    public int TokensPerPeriod { get; set; }

    public int ReplenishmentPeriodInSeconds { get; set; }

    // HTTP response
    public int HttpStatusCode { get; set; } = 429;

    [MaxLength(RateLimitPolicyConsts.MaxCustomMessageLength)]
    public string CustomMessage { get; set; }
}
