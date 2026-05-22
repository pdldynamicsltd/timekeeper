using System;

namespace CadentManagement.TimeTracking.TimeEntries.Dto;

[Serializable]
public class SchedulerEntryDto
{
    public int Id { get; set; }

    public string Text { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Color { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; }

    public int? TaskId { get; set; }

    public string TaskName { get; set; }

    public string Description { get; set; }
}
