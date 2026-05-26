using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using CadentManagement.Authorization.Users;

namespace CadentManagement.TimeTracking.Tasks;

public class ProjectTask : FullAuditedEntity<int>, IMustHaveTenant
{
    public const int MaxNameLength = 256;
    public const int MaxDescriptionLength = 2000;

    public int TenantId { get; set; }

    public virtual int ProjectId { get; set; }

    public virtual Projects.Project Project { get; set; }

    [Required]
    [MaxLength(MaxNameLength)]
    public virtual string Name { get; set; }

    [MaxLength(MaxDescriptionLength)]
    public virtual string Description { get; set; }

    public virtual decimal BudgetHours { get; set; }

    public virtual TaskStatus Status { get; set; }

    public virtual long? AssignedToUserId { get; set; }

    public virtual User AssignedToUser { get; set; }

    public virtual int? ParentTaskId { get; set; }

    public virtual ProjectTask ParentTask { get; set; }

    public virtual ICollection<ProjectTask> SubTasks { get; set; }

    public virtual TaskBudgetTracking BudgetTracking { get; set; }
}
