using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using CadentManagement.TimeTracking.Tasks;

namespace CadentManagement.TimeTracking.Projects;

public class Project : FullAuditedEntity<int>, IMustHaveTenant
{
    public const int MaxNameLength = 256;
    public const int MaxDescriptionLength = 2000;
    public const int MaxColorLength = 10;

    public int TenantId { get; set; }

    [Required]
    [MaxLength(MaxNameLength)]
    public virtual string Name { get; set; }

    [MaxLength(MaxDescriptionLength)]
    public virtual string Description { get; set; }

    public virtual BudgetType BudgetType { get; set; }

    public virtual decimal BudgetHours { get; set; }

    public virtual ProjectStatus Status { get; set; }

    public virtual DateTime? StartDate { get; set; }

    public virtual DateTime? EndDate { get; set; }

    public virtual bool IsPublic { get; set; }

    [MaxLength(MaxColorLength)]
    public virtual string Color { get; set; }

    public virtual ICollection<ProjectTask> Tasks { get; set; }

    public virtual ProjectBudgetTracking BudgetTracking { get; set; }
}
