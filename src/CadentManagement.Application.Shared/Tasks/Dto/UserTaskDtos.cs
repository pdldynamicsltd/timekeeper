using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using CadentManagement.UserTasks;

namespace CadentManagement.UserTasks.Dto;

public class UserTaskDto : EntityDto<int>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public KanbanTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public int? EstimatedMinutes { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int SortOrder { get; set; }
    public string Tags { get; set; }
    public int? ProjectId { get; set; }
    public string ProjectName { get; set; }
    public int? ProjectTaskId { get; set; }
    public string ProjectTaskName { get; set; }
}

public class CreateOrEditUserTaskDto
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public KanbanTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public int? EstimatedMinutes { get; set; }
    public DateTime? DueDate { get; set; }
    public string Tags { get; set; }
    public int? ProjectId { get; set; }
    public int? ProjectTaskId { get; set; }
}

public class GetUserTasksInput : PagedResultRequestDto
{
    public string Filter { get; set; }
    public KanbanTaskStatus? StatusFilter { get; set; }
    public int? ProjectId { get; set; }
}

public class GetUserTaskForEditOutput
{
    public CreateOrEditUserTaskDto Task { get; set; }
    public List<ComboboxItemDto> ProjectOptions { get; set; }
    public List<ComboboxItemDto> ProjectTaskOptions { get; set; }
    public List<ComboboxItemDto> StatusOptions { get; set; }
    public List<ComboboxItemDto> PriorityOptions { get; set; }
}

public class UpdateTaskStatusInput
{
    public int TaskId { get; set; }
    public KanbanTaskStatus NewStatus { get; set; }
    public int NewSortOrder { get; set; }
}

public class ConvertTaskToTimeEntryInput
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public int? ProjectTaskId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Description { get; set; }
}
