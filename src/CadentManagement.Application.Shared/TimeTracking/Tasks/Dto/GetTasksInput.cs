using System;
using Abp.Application.Services.Dto;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Tasks.Dto;

[Serializable]
public class GetTasksInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }

    public int? ProjectId { get; set; }

    public TaskStatus? StatusFilter { get; set; }

    public int? ParentTaskId { get; set; }
}
