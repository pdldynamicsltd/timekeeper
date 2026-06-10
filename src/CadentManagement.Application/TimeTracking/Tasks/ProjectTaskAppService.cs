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
using CadentManagement.Authorization.Users;
using CadentManagement.Mappers;
using CadentManagement.TimeTracking.Tasks;
using CadentManagement.TimeTracking.Tasks.Dto;
using CadentManagement.TimeTracking.Projects;

namespace CadentManagement.TimeTracking.Tasks;

[AbpAuthorize(AppPermissions.Pages_TimeTracking_Tasks)]
public class ProjectTaskAppService : CadentManagementAppServiceBase, IProjectTaskAppService
{
    private readonly IRepository<ProjectTask, int> _taskRepository;
    private readonly IRepository<TaskBudgetTracking, int> _budgetTrackingRepository;
    private readonly IRepository<Project, int> _projectRepository;
    private readonly IRepository<User, long> _userRepository;
    private readonly ProjectTaskToTaskDtoMapper _toTaskDtoMapper;
    private readonly CreateOrEditTaskDtoToProjectTaskMapper _toTaskMapper;

    public ProjectTaskAppService(
        IRepository<ProjectTask, int> taskRepository,
        IRepository<TaskBudgetTracking, int> budgetTrackingRepository,
        IRepository<Project, int> projectRepository,
        IRepository<User, long> userRepository,
        ProjectTaskToTaskDtoMapper toTaskDtoMapper,
        CreateOrEditTaskDtoToProjectTaskMapper toTaskMapper)
    {
        _taskRepository = taskRepository;
        _budgetTrackingRepository = budgetTrackingRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _toTaskDtoMapper = toTaskDtoMapper;
        _toTaskMapper = toTaskMapper;
    }

    public async Task<GetTaskForEditOutput> GetForEditAsync(NullableIdDto<int> input)
    {
        var projects = await _projectRepository.GetAllListAsync(p => p.TenantId == AbpSession.TenantId);
        var users = await _userRepository.GetAllListAsync(u => u.TenantId == AbpSession.TenantId);

        var output = new GetTaskForEditOutput
        {
            ProjectOptions = projects.Select(p => new ComboboxItemDto(p.Id.ToString(), p.Name)).ToList(),
            StatusOptions = GetStatusOptions(),
            UserOptions = users.Select(u => new ComboboxItemDto(u.Id.ToString(), u.FullName)).ToList()
        };

        if (input.Id.HasValue)
        {
            var task = await _taskRepository.GetAsync(input.Id.Value);
            output.Task = new CreateOrEditTaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Name = task.Name,
                Description = task.Description,
                BudgetHours = task.BudgetHours,
                Status = task.Status,
                AssignedToUserId = task.AssignedToUserId,
                ParentTaskId = task.ParentTaskId
            };

            var siblingTasks = await _taskRepository.GetAllListAsync(t =>
                t.ProjectId == task.ProjectId && t.Id != task.Id && t.ParentTaskId == null);
            output.ParentTaskOptions = siblingTasks.Select(t => new ComboboxItemDto(t.Id.ToString(), t.Name)).ToList();
        }
        else
        {
            output.Task = new CreateOrEditTaskDto
            {
                Status = TaskStatus.Active
            };
        }

        return output;
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_Tasks_Create)]
    public async Task<int> CreateAsync(CreateOrEditTaskDto input)
    {
        var task = _toTaskMapper.Map(input);
        task.TenantId = AbpSession.TenantId ?? 0;

        var taskId = await _taskRepository.InsertAndGetIdAsync(task);

        if (input.BudgetHours > 0)
        {
            await _budgetTrackingRepository.InsertAsync(new TaskBudgetTracking
            {
                TaskId = taskId,
                TotalBudgetHours = input.BudgetHours ?? 0,
                UsedHours = 0,
                RemainingHours = input.BudgetHours ?? 0
            });
        }

        return taskId;
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_Tasks_Edit)]
    public async Task UpdateAsync(CreateOrEditTaskDto input)
    {
        var task = await _taskRepository.GetAsync(input.Id.Value);
        _toTaskMapper.Map(input, task);

        var tracking = await _budgetTrackingRepository.FirstOrDefaultAsync(b => b.TaskId == input.Id.Value);
        if (tracking != null)
        {
            tracking.TotalBudgetHours = input.BudgetHours ?? 0;
            tracking.RemainingHours = (input.BudgetHours ?? 0) - tracking.UsedHours;
        }
        else if (input.BudgetHours > 0)
        {
            await _budgetTrackingRepository.InsertAsync(new TaskBudgetTracking
            {
                TaskId = input.Id.Value,
                TotalBudgetHours = input.BudgetHours ?? 0,
                UsedHours = 0,
                RemainingHours = input.BudgetHours ?? 0
            });
        }
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_Tasks_Delete)]
    public async Task DeleteAsync(EntityDto<int> input)
    {
        await _taskRepository.DeleteAsync(input.Id);
    }

    [HttpPost]
    public async Task<PagedResultDto<TaskDto>> GetTasksAsync(GetTasksInput input)
    {
        var query = _taskRepository.GetAll()
            .Where(t => t.TenantId == AbpSession.TenantId)
            .WhereIf(!string.IsNullOrEmpty(input.Filter), t => t.Name.Contains(input.Filter))
            .WhereIf(input.ProjectId.HasValue, t => t.ProjectId == input.ProjectId.Value)
            .WhereIf(input.StatusFilter.HasValue, t => t.Status == input.StatusFilter.Value)
            .WhereIf(input.ParentTaskId.HasValue, t => t.ParentTaskId == input.ParentTaskId.Value);

        var totalCount = await query.CountAsync();
        var tasks = await query
            .Include(t => t.Project)
            .Include(t => t.BudgetTracking)
            .Include(t => t.AssignedToUser)
            .OrderBy(t => t.Name)
            .PageBy(input)
            .ToListAsync();

        var taskDtos = tasks.Select(t =>
        {
            var dto = _toTaskDtoMapper.Map(t);
            dto.ProjectName = t.Project?.Name;
            dto.AssignedToUserName = t.AssignedToUser?.FullName;
            if (t.BudgetTracking != null)
            {
                dto.UsedHours = t.BudgetTracking.UsedHours;
                dto.RemainingHours = t.BudgetTracking.RemainingHours;
            }
            return dto;
        }).ToList();

        return new PagedResultDto<TaskDto>(totalCount, taskDtos);
    }

    public async Task<List<TaskDto>> GetProjectTaskTreeAsync(EntityDto<int> input)
    {
        var tasks = await _taskRepository.GetAll()
            .Where(t => t.ProjectId == input.Id)
            .Include(t => t.BudgetTracking)
            .Include(t => t.AssignedToUser)
            .ToListAsync();

        return BuildTaskTree(tasks, null);
    }

    public async Task<List<TaskDto>> GetTasksForProjectAsync(int projectId)
    {
        var tasks = await _taskRepository.GetAllListAsync(t => t.ProjectId == projectId);
        return tasks.Select(t => _toTaskDtoMapper.Map(t)).ToList();
    }

    private List<TaskDto> BuildTaskTree(List<ProjectTask> allTasks, int? parentId)
    {
        return allTasks
            .Where(t => t.ParentTaskId == parentId)
            .Select(t =>
            {
                var dto = _toTaskDtoMapper.Map(t);
                dto.AssignedToUserName = t.AssignedToUser?.FullName;
                if (t.BudgetTracking != null)
                {
                    dto.UsedHours = t.BudgetTracking.UsedHours;
                    dto.RemainingHours = t.BudgetTracking.RemainingHours;
                }
                dto.SubTasks = BuildTaskTree(allTasks, t.Id);
                return dto;
            })
            .ToList();
    }

    private List<ComboboxItemDto> GetStatusOptions()
    {
        return new List<ComboboxItemDto>
        {
            new ComboboxItemDto(((int)TaskStatus.Active).ToString(), L("ActiveStatus")),
            new ComboboxItemDto(((int)TaskStatus.Archived).ToString(), L("ArchivedStatus")),
            new ComboboxItemDto(((int)TaskStatus.Completed).ToString(), L("CompletedStatus"))
        };
    }
}
