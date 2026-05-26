using System;
using Abp.Application.Services.Dto;

namespace CadentManagement.TimeTracking.TimeEntries.Dto;

[Serializable]
public class GetTimeEntriesInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }

    public int? ProjectId { get; set; }

    public int? TaskId { get; set; }

    public long? UserId { get; set; }

    public DateTime? StartDateFilter { get; set; }

    public DateTime? EndDateFilter { get; set; }
}
