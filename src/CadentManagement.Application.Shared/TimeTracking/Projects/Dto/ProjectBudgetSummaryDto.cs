using System;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Projects.Dto;

[Serializable]
public class ProjectBudgetSummaryDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; }

    public ProjectStatus Status { get; set; }

    public BudgetType BudgetType { get; set; }

    public decimal TotalBudgetHours { get; set; }

    public decimal UsedHours { get; set; }

    public decimal RemainingHours { get; set; }

    public decimal UtilizationPercentage { get; set; }

    public bool IsBudgetExceeded => RemainingHours < 0;

    public bool IsNearingLimit => UtilizationPercentage >= 80 && !IsBudgetExceeded;
}
