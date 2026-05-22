using System;
using System.ComponentModel.DataAnnotations;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Tasks.Dto;

[Serializable]
public class CreateOrEditTaskDto
{
    public int? Id { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    public decimal? BudgetHours { get; set; }

    public TaskStatus Status { get; set; }

    public long? AssignedToUserId { get; set; }

    public int? ParentTaskId { get; set; }
}
