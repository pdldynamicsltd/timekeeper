using System;

namespace CadentManagement.TimeTracking.TimeEntries.Dto;

[Serializable]
public class GetSchedulerEntriesInput
{
    public int? ProjectId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
