using System;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Projects.Dto;

[Serializable]
public class ProjectDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public BudgetType BudgetType { get; set; }

    public decimal BudgetHours { get; set; }

    public ProjectStatus Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsPublic { get; set; }

    public string Color { get; set; }

    public decimal UsedHours { get; set; }

    public decimal RemainingHours { get; set; }

    public decimal UtilizationPercentage { get; set; }
}
