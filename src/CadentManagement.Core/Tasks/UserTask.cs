using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.Tasks;

namespace CadentManagement.UserTasks;

public class UserTask : FullAuditedEntity<int>, IMustHaveTenant
{
    public const int MaxTitleLength = 256;
    public const int MaxDescriptionLength = 2000;
    public const int MaxTagsLength = 500;

    public int TenantId { get; set; }

    /// <summary>Owner — set from AbpSession.UserId on create.</summary>
    public long UserId { get; set; }

    [Required]
    [MaxLength(MaxTitleLength)]
    public string Title { get; set; }

    [MaxLength(MaxDescriptionLength)]
    public string Description { get; set; }

    public KanbanTaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    /// <summary>Estimated work in minutes.</summary>
    public int? EstimatedMinutes { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>Sort order within the current status column.</summary>
    public int SortOrder { get; set; }

    [MaxLength(MaxTagsLength)]
    public string Tags { get; set; }

    // ─── Optional project linkage ─────────────────────────────────────

    public int? ProjectId { get; set; }

    public virtual Project Project { get; set; }

    public int? ProjectTaskId { get; set; }

    public virtual ProjectTask ProjectTask { get; set; }

    // ─── Time entry conversion ────────────────────────────────────────

    public bool IsConvertedToTimeEntry { get; set; }

    public int? ConvertedTimeEntryId { get; set; }
}
