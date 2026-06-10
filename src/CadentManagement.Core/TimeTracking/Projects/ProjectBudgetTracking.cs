using System;
using Abp.Domain.Entities.Auditing;

namespace CadentManagement.TimeTracking.Projects;

public class ProjectBudgetTracking : FullAuditedEntity<int>
{
    public virtual int ProjectId { get; set; }

    public virtual Project Project { get; set; }

    public virtual decimal TotalBudgetHours { get; set; }

    public virtual decimal UsedHours { get; set; }

    public virtual decimal RemainingHours { get; set; }

    public virtual decimal UtilizationPercentage => TotalBudgetHours > 0
        ? Math.Min(UsedHours / TotalBudgetHours * 100, 100)
        : 0;
}
