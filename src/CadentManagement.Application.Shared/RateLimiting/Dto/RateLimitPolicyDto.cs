using Abp.Application.Services.Dto;

namespace CadentManagement.RateLimiting.Dto;

public class RateLimitPolicyDto : EntityDto
{
    public string Name { get; set; }

    public bool IsEnabled { get; set; }

    public RateLimitAlgorithm Algorithm { get; set; }

    public RateLimitPartitionType PartitionType { get; set; }

    public bool IsGlobal { get; set; }

    public string EndpointPattern { get; set; }

    public int PermitLimit { get; set; }

    public int WindowInSeconds { get; set; }

    public int QueueLimit { get; set; }

    public int SegmentsPerWindow { get; set; }

    public int TokensPerPeriod { get; set; }

    public int ReplenishmentPeriodInSeconds { get; set; }

    public int HttpStatusCode { get; set; }

    public string CustomMessage { get; set; }
}
