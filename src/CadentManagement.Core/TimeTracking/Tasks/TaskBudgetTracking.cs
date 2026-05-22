using System;
using Abp.Domain.Entities.Auditing;

namespace CadentManagement.TimeTracking.Tasks;

public class TaskBudgetTracking : FullAuditedEntity<int>
{
    public virtual int TaskId { get; set; }

    public virtual ProjectTask Task { get; set; }

    public virtual decimal TotalBudgetHours { get; set; }

    public virtual decimal UsedHours { get; set; }

    public virtual decimal RemainingHours { get; set; }

    public virtual decimal UtilizationPercentage => TotalBudgetHours > 0
        ? Math.Min(UsedHours / TotalBudgetHours * 100, 100)
        : 0;
}
