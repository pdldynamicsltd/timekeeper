using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace CadentManagement.UserTasks;

public class TodoStatus : Entity<int>, IMustHaveTenant
{
    public const int MaxNameLength = 64;
    public const int MaxColorLength = 16;

    public int TenantId { get; set; }

    /// <summary>Stable integer value saved on ToDo items.</summary>
    public int Value { get; set; }

    [Required]
    [MaxLength(MaxNameLength)]
    public string Name { get; set; }

    [MaxLength(MaxColorLength)]
    public string Color { get; set; }

    public int SortOrder { get; set; }

    public bool IsCompleted { get; set; }
}