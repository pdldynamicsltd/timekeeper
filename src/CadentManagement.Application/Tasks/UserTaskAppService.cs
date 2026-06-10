using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CadentManagement.Authorization;
using CadentManagement.Mappers;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.TimeEntries;
using CadentManagement.UserTasks.Dto;

namespace CadentManagement.UserTasks;

[AbpAuthorize(AppPermissions.Pages_Tasks)]
public class UserTaskAppService : CadentManagementAppServiceBase, IUserTaskAppService
{
    private readonly IRepository<UserTask, int> _taskRepository;
    private readonly IRepository<TodoStatus, int> _todoStatusRepository;
    private readonly IRepository<Project, int> _projectRepository;
    private readonly IRepository<TimeTracking.Tasks.ProjectTask, int> _projectTaskRepository;
    private readonly IRepository<TimeEntry, int> _timeEntryRepository;
    private readonly UserTaskToUserTaskDtoMapper _toUserTaskDtoMapper;
    private readonly CreateOrEditUserTaskDtoToUserTaskMapper _toUserTaskMapper;

    public UserTaskAppService(
        IRepository<UserTask, int> taskRepository,
        IRepository<TodoStatus, int> todoStatusRepository,
        IRepository<Project, int> projectRepository,
        IRepository<TimeTracking.Tasks.ProjectTask, int> projectTaskRepository,
        IRepository<TimeEntry, int> timeEntryRepository,
        UserTaskToUserTaskDtoMapper toUserTaskDtoMapper,
        CreateOrEditUserTaskDtoToUserTaskMapper toUserTaskMapper)
    {
        _taskRepository = taskRepository;
        _todoStatusRepository = todoStatusRepository;
        _projectRepository = projectRepository;
        _projectTaskRepository = projectTaskRepository;
        _timeEntryRepository = timeEntryRepository;
        _toUserTaskDtoMapper = toUserTaskDtoMapper;
        _toUserTaskMapper = toUserTaskMapper;
    }

    public async Task<GetUserTaskForEditOutput> GetForEditAsync(NullableIdDto<int> input)
    {
        await EnsureDefaultStatusesAsync();

        var projects = await _projectRepository.GetAllListAsync();
        var projectTasks = await _projectTaskRepository.GetAllListAsync();

        var output = new GetUserTaskForEditOutput
        {
            ProjectOptions = projects.Select(p => new ComboboxItemDto(p.Id.ToString(), p.Name)).ToList(),
            ProjectTaskOptions = new List<ComboboxItemDto>(),
            StatusOptions = await GetStatusOptionsAsync(),
            PriorityOptions = GetPriorityOptions()
        };

        if (input.Id.HasValue)
        {
            var task = await _taskRepository.GetAsync(input.Id.Value);
            output.Task = new CreateOrEditUserTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                EstimatedMinutes = task.EstimatedMinutes,
                DueDate = task.DueDate,
                Tags = task.Tags,
                ProjectId = task.ProjectId,
                ProjectTaskId = task.ProjectTaskId
            };

            if (task.ProjectId.HasValue)
            {
                output.ProjectTaskOptions = projectTasks
                    .Where(t => t.ProjectId == task.ProjectId.Value)
                    .Select(t => new ComboboxItemDto(t.Id.ToString(), t.Name))
                    .ToList();
            }
        }
        else
        {
            output.Task = new CreateOrEditUserTaskDto
            {
                Status = (KanbanTaskStatus)await GetDefaultStatusValueAsync(),
                Priority = TaskPriority.Medium
            };
        }

        return output;
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Create)]
    public async Task<int> CreateAsync(CreateOrEditUserTaskDto input)
    {
        await EnsureDefaultStatusesAsync();

        var task = _toUserTaskMapper.Map(input);
        task.TenantId = AbpSession.TenantId ?? 0;
        task.UserId = AbpSession.UserId ?? 0;
        task.SortOrder = 0;

        var isCompleted = await IsCompletedStatusAsync((int)input.Status) || input.Status == KanbanTaskStatus.Done;
        task.CompletedAt = isCompleted ? DateTime.Now : null;

        return await _taskRepository.InsertAndGetIdAsync(task);
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Edit)]
    public async Task UpdateAsync(CreateOrEditUserTaskDto input)
    {
        await EnsureDefaultStatusesAsync();

        var task = await _taskRepository.GetAsync(input.Id.Value);
        _toUserTaskMapper.Map(input, task);

        var isCompleted = await IsCompletedStatusAsync((int)task.Status) || task.Status == KanbanTaskStatus.Done;
        task.CompletedAt = isCompleted ? task.CompletedAt ?? DateTime.Now : null;
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Delete)]
    public async Task DeleteAsync(EntityDto<int> input)
    {
        await _taskRepository.DeleteAsync(input.Id);
    }

    [HttpPost]
    public async Task<PagedResultDto<UserTaskDto>> GetTasksAsync(GetUserTasksInput input)
    {
        await EnsureDefaultStatusesAsync();

        var statuses = await GetSortedStatusesAsync();
        var statusNameByValue = statuses.ToDictionary(s => s.Value, s => s.Name);

        var query = _taskRepository.GetAll()
            .Where(t => t.UserId == AbpSession.UserId)
            .WhereIf(!string.IsNullOrEmpty(input.Filter), t => t.Title.Contains(input.Filter))
            .WhereIf(input.StatusFilter.HasValue, t => (int)t.Status == input.StatusFilter.Value)
            .WhereIf(input.ProjectId.HasValue, t => t.ProjectId == input.ProjectId.Value);

        var totalCount = await query.CountAsync();
        var tasks = await query
            .Include(t => t.Project)
            .Include(t => t.ProjectTask)
            .OrderBy(t => t.Status).ThenBy(t => t.SortOrder).ThenBy(t => t.CreationTime)
            .PageBy(input)
            .ToListAsync();

        var taskDtos = tasks.Select(t =>
        {
            var dto = _toUserTaskDtoMapper.Map(t);
            dto.ProjectName = t.Project?.Name;
            dto.ProjectTaskName = t.ProjectTask?.Name;
            dto.StatusName = statusNameByValue.TryGetValue((int)t.Status, out var statusName) ? statusName : L("TodoStatus");
            return dto;
        }).ToList();

        return new PagedResultDto<UserTaskDto>(totalCount, taskDtos);
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Edit)]
    public async Task UpdateStatusAsync(UpdateTaskStatusInput input)
    {
        await EnsureDefaultStatusesAsync();

        var task = await _taskRepository.GetAsync(input.TaskId);
        task.Status = (KanbanTaskStatus)input.NewStatus;
        task.SortOrder = input.NewSortOrder;

        var isCompleted = await IsCompletedStatusAsync(input.NewStatus) || input.NewStatus == (int)KanbanTaskStatus.Done;
        task.CompletedAt = isCompleted ? task.CompletedAt ?? DateTime.Now : null;
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Edit)]
    public async Task UpdateDueDateAsync(UpdateTaskDueDateInput input)
    {
        var task = await _taskRepository.GetAsync(input.TaskId);
        task.DueDate = input.DueDate;
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Edit)]
    public async Task CompleteAsync(EntityDto<int> input)
    {
        await EnsureDefaultStatusesAsync();

        var task = await _taskRepository.GetAsync(input.Id);
        var completedStatusValue = await GetFirstCompletedStatusValueAsync();

        if (!completedStatusValue.HasValue)
        {
            completedStatusValue = (int)KanbanTaskStatus.Done;
        }

        task.Status = (KanbanTaskStatus)completedStatusValue.Value;
        task.CompletedAt = task.CompletedAt ?? DateTime.Now;
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_TimeEntries_Create)]
    public async Task ConvertToTimeEntryAsync(ConvertTaskToTimeEntryInput input)
    {
        var task = await _taskRepository.GetAsync(input.TaskId);

        if (task.IsConvertedToTimeEntry)
        {
            throw new UserFriendlyException(L("TaskAlreadyConvertedToTimeEntry"));
        }

        var timeEntry = new TimeEntry
        {
            TenantId = task.TenantId,
            UserId = task.UserId,
            ProjectId = input.ProjectId,
            TaskId = input.ProjectTaskId,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Description = input.Description ?? task.Title
        };

        var timeEntryId = await _timeEntryRepository.InsertAndGetIdAsync(timeEntry);

        task.IsConvertedToTimeEntry = true;
        task.ConvertedTimeEntryId = timeEntryId;
        await _taskRepository.UpdateAsync(task);
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks)]
    public async Task<List<TodoStatusDto>> GetTodoStatusesAsync()
    {
        await EnsureDefaultStatusesAsync();

        var statuses = await GetSortedStatusesAsync();
        return statuses.Select(s => new TodoStatusDto
        {
            Id = s.Id,
            Value = s.Value,
            Name = s.Name,
            Color = s.Color,
            SortOrder = s.SortOrder,
            IsCompleted = s.IsCompleted
        }).ToList();
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Create)]
    public async Task<int> CreateTodoStatusAsync(CreateOrEditTodoStatusDto input)
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var nextValue = await _todoStatusRepository.GetAll()
            .Select(s => (int?)s.Value)
            .MaxAsync() ?? 0;

        var nextSortOrder = input.SortOrder > 0
            ? input.SortOrder
            : (await _todoStatusRepository.GetAll()
                .Select(s => (int?)s.SortOrder)
                .MaxAsync() ?? 0) + 10;

        var status = new TodoStatus
        {
            TenantId = tenantId,
            Value = nextValue + 1,
            Name = input.Name,
            Color = input.Color,
            SortOrder = nextSortOrder,
            IsCompleted = input.IsCompleted
        };

        return await _todoStatusRepository.InsertAndGetIdAsync(status);
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Edit)]
    public async Task UpdateTodoStatusAsync(CreateOrEditTodoStatusDto input)
    {
        var status = await _todoStatusRepository.GetAsync(input.Id.Value);
        status.Name = input.Name;
        status.Color = input.Color;
        status.SortOrder = input.SortOrder;
        status.IsCompleted = input.IsCompleted;
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Delete)]
    public async Task DeleteTodoStatusAsync(EntityDto<int> input)
    {
        var status = await _todoStatusRepository.GetAsync(input.Id);

        var inUse = await _taskRepository.GetAll()
            .AnyAsync(t => (int)t.Status == status.Value);

        if (inUse)
        {
            throw new UserFriendlyException(L("TodoStatusInUse"));
        }

        await _todoStatusRepository.DeleteAsync(status);
    }

    private async Task<int> GetDefaultStatusValueAsync()
    {
        var statuses = await GetSortedStatusesAsync();
        return statuses.Any() ? statuses.First().Value : (int)KanbanTaskStatus.Todo;
    }

    private async Task<List<ComboboxItemDto>> GetStatusOptionsAsync()
    {
        var statuses = await GetSortedStatusesAsync();
        return statuses
            .Select(s => new ComboboxItemDto(s.Value.ToString(), s.Name))
            .ToList();
    }

    private async Task<List<TodoStatus>> GetSortedStatusesAsync()
    {
        return await _todoStatusRepository.GetAll()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Value)
            .ToListAsync();
    }

    private async Task<bool> IsCompletedStatusAsync(int statusValue)
    {
        var status = await _todoStatusRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Value == statusValue);

        return status?.IsCompleted ?? statusValue == (int)KanbanTaskStatus.Done;
    }

    private async Task<int?> GetFirstCompletedStatusValueAsync()
    {
        return await _todoStatusRepository.GetAll()
            .Where(s => s.IsCompleted)
            .OrderBy(s => s.SortOrder)
            .Select(s => (int?)s.Value)
            .FirstOrDefaultAsync();
    }

    private async Task EnsureDefaultStatusesAsync()
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var hasAnyStatus = await _todoStatusRepository.GetAll().AnyAsync();
        if (hasAnyStatus)
        {
            return;
        }

        await _todoStatusRepository.InsertAsync(new TodoStatus
        {
            TenantId = tenantId,
            Value = (int)KanbanTaskStatus.Backlog,
            Name = L("BacklogStatus"),
            SortOrder = 10,
            Color = "#6c757d",
            IsCompleted = false
        });

        await _todoStatusRepository.InsertAsync(new TodoStatus
        {
            TenantId = tenantId,
            Value = (int)KanbanTaskStatus.Todo,
            Name = L("TodoStatus"),
            SortOrder = 20,
            Color = "#0d6efd",
            IsCompleted = false
        });

        await _todoStatusRepository.InsertAsync(new TodoStatus
        {
            TenantId = tenantId,
            Value = (int)KanbanTaskStatus.InProgress,
            Name = L("InProgressStatus"),
            SortOrder = 30,
            Color = "#fd7e14",
            IsCompleted = false
        });

        await _todoStatusRepository.InsertAsync(new TodoStatus
        {
            TenantId = tenantId,
            Value = (int)KanbanTaskStatus.Done,
            Name = L("DoneStatus"),
            SortOrder = 40,
            Color = "#198754",
            IsCompleted = true
        });
    }

    private List<ComboboxItemDto> GetPriorityOptions()
    {
        return new List<ComboboxItemDto>
        {
            new ComboboxItemDto(((int)TaskPriority.Low).ToString(), L("LowPriority")),
            new ComboboxItemDto(((int)TaskPriority.Medium).ToString(), L("MediumPriority")),
            new ComboboxItemDto(((int)TaskPriority.High).ToString(), L("HighPriority")),
            new ComboboxItemDto(((int)TaskPriority.Urgent).ToString(), L("UrgentPriority"))
        };
    }
}