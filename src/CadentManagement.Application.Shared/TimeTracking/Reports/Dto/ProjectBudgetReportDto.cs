using System;
using System.Collections.Generic;
using CadentManagement.TimeTracking.Projects.Dto;

namespace CadentManagement.TimeTracking.Reports.Dto;

[Serializable]
public class ProjectBudgetReportDto
{
    public ProjectBudgetSummaryDto BudgetSummary { get; set; }

    public List<TaskBudgetSummaryDto> TaskBudgets { get; set; } = new();
}

[Serializable]
public class TaskBudgetSummaryDto
{
    public int TaskId { get; set; }

    public string TaskName { get; set; }

    public int? ParentTaskId { get; set; }

    public decimal TotalBudgetHours { get; set; }

    public decimal UsedHours { get; set; }

    public decimal RemainingHours { get; set; }

    public decimal UtilizationPercentage { get; set; }

    public bool IsBudgetExceeded => RemainingHours < 0;
}
