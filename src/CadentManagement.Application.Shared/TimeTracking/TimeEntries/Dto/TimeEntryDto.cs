using System;

namespace CadentManagement.TimeTracking.TimeEntries.Dto;

[Serializable]
public class TimeEntryDto
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; }

    public string ProjectColor { get; set; }

    public int? TaskId { get; set; }

    public string TaskName { get; set; }

    public long UserId { get; set; }

    public string UserName { get; set; }

    public string UserFullName { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal DurationHours { get; set; }

    public string Description { get; set; }
}
