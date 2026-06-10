using System;
using System.ComponentModel.DataAnnotations;

namespace CadentManagement.TimeTracking.TimeEntries.Dto;

[Serializable]
public class CreateOrEditTimeEntryDto
{
    public int? Id { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public int? TaskId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }
}
