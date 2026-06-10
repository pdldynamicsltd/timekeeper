using System;
using System.Collections.Generic;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Tasks.Dto;

[Serializable]
public class TaskDto
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal BudgetHours { get; set; }

    public TaskStatus Status { get; set; }

    public long? AssignedToUserId { get; set; }

    public string AssignedToUserName { get; set; }

    public int? ParentTaskId { get; set; }

    public string ParentTaskName { get; set; }

    public decimal UsedHours { get; set; }

    public decimal RemainingHours { get; set; }

    public List<TaskDto> SubTasks { get; set; } = new();
}
