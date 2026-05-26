using System;
using System.ComponentModel.DataAnnotations;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Projects.Dto;

[Serializable]
public class CreateOrEditProjectDto
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    public BudgetType BudgetType { get; set; }

    public decimal? BudgetHours { get; set; }

    public ProjectStatus Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsPublic { get; set; }

    [MaxLength(10)]
    public string Color { get; set; }
}
