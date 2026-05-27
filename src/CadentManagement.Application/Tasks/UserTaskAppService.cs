using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CadentManagement.Authorization;
using CadentManagement.Mappers;
using CadentManagement.UserTasks;
using CadentManagement.UserTasks.Dto;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.TimeEntries;

namespace CadentManagement.UserTasks;

[AbpAuthorize(AppPermissions.Pages_Tasks)]
public class UserTaskAppService : CadentManagementAppServiceBase, IUserTaskAppService
{
    private readonly IRepository<UserTask, int> _taskRepository;
    private readonly IRepository<Project, int> _projectRepository;
    private readonly IRepository<TimeTracking.Tasks.ProjectTask, int> _projectTaskRepository;
    private readonly IRepository<TimeEntry, int> _timeEntryRepository;
    private readonly UserTaskToUserTaskDtoMapper _toUserTaskDtoMapper;
    private readonly CreateOrEditUserTaskDtoToUserTaskMapper _toUserTaskMapper;

    public UserTaskAppService(
        IRepository<UserTask, int> taskRepository,
        IRepository<Project, int> projectRepository,
        IRepository<TimeTracking.Tasks.ProjectTask, int> projectTaskRepository,
        IRepository<TimeEntry, int> timeEntryRepository,
        UserTaskToUserTaskDtoMapper toUserTaskDtoMapper,
        CreateOrEditUserTaskDtoToUserTaskMapper toUserTaskMapper)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _projectTaskRepository = projectTaskRepository;
        _timeEntryRepository = timeEntryRepository;
        _toUserTaskDtoMapper = toUserTaskDtoMapper;
        _toUserTaskMapper = toUserTaskMapper;
    }

    public async Task<GetUserTaskForEditOutput> GetForEditAsync(NullableIdDto<int> input)
    {
        var projects = await _projectRepository.GetAllListAsync(p => p.TenantId == AbpSession.TenantId);
        var projectTasks = await _projectTaskRepository.GetAllListAsync(t => t.Project.TenantId == AbpSession.TenantId);

        var output = new GetUserTaskForEditOutput
        {
            ProjectOptions = projects.Select(p => new ComboboxItemDto(p.Id.ToString(), p.Name)).ToList(),
            ProjectTaskOptions = new List<ComboboxItemDto>(),
            StatusOptions = GetStatusOptions(),
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
            
            // Load related project tasks if project is selected
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
                Status = KanbanTaskStatus.Backlog,
                Priority = TaskPriority.Medium
            };
        }

        return output;
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Create)]
    public async Task<int> CreateAsync(CreateOrEditUserTaskDto input)
    {
        var task = _toUserTaskMapper.Map(input);
        task.TenantId = AbpSession.TenantId ?? 0;
        task.UserId = AbpSession.UserId ?? 0;
        task.SortOrder = 0;
        task.CompletedAt = input.Status == KanbanTaskStatus.Done ? DateTime.Now : null;

        var taskId = await _taskRepository.InsertAndGetIdAsync(task);
        return taskId;
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Edit)]
    public async Task UpdateAsync(CreateOrEditUserTaskDto input)
    {
        var task = await _taskRepository.GetAsync(input.Id.Value);
        _toUserTaskMapper.Map(input, task);
        task.CompletedAt = task.Status == KanbanTaskStatus.Done ? (task.CompletedAt ?? DateTime.Now) : null;
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Delete)]
    public async Task DeleteAsync(EntityDto<int> input)
    {
        await _taskRepository.DeleteAsync(input.Id);
    }

    [HttpPost]
    public async Task<PagedResultDto<UserTaskDto>> GetTasksAsync(GetUserTasksInput input)
    {
        var query = _taskRepository.GetAll()
            .Where(t => t.TenantId == AbpSession.TenantId && t.UserId == AbpSession.UserId)
            .WhereIf(!string.IsNullOrEmpty(input.Filter), t => t.Title.Contains(input.Filter))
            .WhereIf(input.StatusFilter.HasValue, t => t.Status == input.StatusFilter.Value)
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
            return dto;
        }).ToList();

        return new PagedResultDto<UserTaskDto>(totalCount, taskDtos);
    }

    [AbpAuthorize(AppPermissions.Pages_Tasks_Edit)]
    public async Task UpdateStatusAsync(UpdateTaskStatusInput input)
    {
        var task = await _taskRepository.GetAsync(input.TaskId);
        task.Status = input.NewStatus;
        task.SortOrder = input.NewSortOrder;

        // Auto-set CompletedAt when status changes to Done
        if (input.NewStatus == KanbanTaskStatus.Done)
        {
            task.CompletedAt = task.CompletedAt ?? DateTime.Now;
        }
        else
        {
            task.CompletedAt = null;
        }
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_TimeEntries_Create)]
    public async Task ConvertToTimeEntryAsync(ConvertTaskToTimeEntryInput input)
    {
        var task = await _taskRepository.GetAsync(input.TaskId);

        if (task.IsConvertedToTimeEntry)
        {
            throw new Abp.UI.UserFriendlyException(L("TaskAlreadyConvertedToTimeEntry"));
        }

        // Create time entry
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

        // Mark task as converted
        task.IsConvertedToTimeEntry = true;
        task.ConvertedTimeEntryId = timeEntryId;
        await _taskRepository.UpdateAsync(task);
    }

    private List<ComboboxItemDto> GetStatusOptions()
    {
        return new List<ComboboxItemDto>
        {
            new ComboboxItemDto(((int)KanbanTaskStatus.Backlog).ToString(), L("BacklogStatus")),
            new ComboboxItemDto(((int)KanbanTaskStatus.Todo).ToString(), L("TodoStatus")),
            new ComboboxItemDto(((int)KanbanTaskStatus.InProgress).ToString(), L("InProgressStatus")),
            new ComboboxItemDto(((int)KanbanTaskStatus.Done).ToString(), L("DoneStatus"))
        };
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
