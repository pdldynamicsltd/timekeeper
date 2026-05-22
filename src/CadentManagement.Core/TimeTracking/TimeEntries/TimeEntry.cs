using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using CadentManagement.Authorization.Users;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.Tasks;

namespace CadentManagement.TimeTracking.TimeEntries;

public class TimeEntry : FullAuditedEntity<int>, IMustHaveTenant
{
    public const int MaxDescriptionLength = 2000;

    public int TenantId { get; set; }

    public virtual int ProjectId { get; set; }

    public virtual Project Project { get; set; }

    public virtual int? TaskId { get; set; }

    public virtual ProjectTask Task { get; set; }

    public virtual long UserId { get; set; }

    public virtual User User { get; set; }

    [MaxLength(MaxDescriptionLength)]
    public virtual string Description { get; set; }

    public virtual DateTime StartTime { get; set; }

    public virtual DateTime EndTime { get; set; }

    public virtual decimal DurationHours => EndTime > StartTime
        ? (decimal)(EndTime - StartTime).TotalHours
        : 0;
}
