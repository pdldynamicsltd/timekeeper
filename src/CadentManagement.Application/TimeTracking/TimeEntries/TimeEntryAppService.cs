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
using CadentManagement.TimeTracking.TimeEntries;
using CadentManagement.TimeTracking.TimeEntries.Dto;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.Tasks;

namespace CadentManagement.TimeTracking.TimeEntries;

[AbpAuthorize(AppPermissions.Pages_TimeTracking_TimeEntries)]
public class TimeEntryAppService : CadentManagementAppServiceBase, ITimeEntryAppService
{
    private readonly IRepository<TimeEntry, int> _timeEntryRepository;
    private readonly IRepository<Project, int> _projectRepository;
    private readonly IRepository<ProjectTask, int> _taskRepository;
    private readonly IRepository<ProjectBudgetTracking, int> _projectBudgetRepository;
    private readonly IRepository<TaskBudgetTracking, int> _taskBudgetRepository;
    private readonly TimeEntryToTimeEntryDtoMapper _toTimeEntryDtoMapper;
    private readonly CreateOrEditTimeEntryDtoToTimeEntryMapper _toTimeEntryMapper;

    public TimeEntryAppService(
        IRepository<TimeEntry, int> timeEntryRepository,
        IRepository<Project, int> projectRepository,
        IRepository<ProjectTask, int> taskRepository,
        IRepository<ProjectBudgetTracking, int> projectBudgetRepository,
        IRepository<TaskBudgetTracking, int> taskBudgetRepository,
        TimeEntryToTimeEntryDtoMapper toTimeEntryDtoMapper,
        CreateOrEditTimeEntryDtoToTimeEntryMapper toTimeEntryMapper)
    {
        _timeEntryRepository = timeEntryRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _projectBudgetRepository = projectBudgetRepository;
        _taskBudgetRepository = taskBudgetRepository;
        _toTimeEntryDtoMapper = toTimeEntryDtoMapper;
        _toTimeEntryMapper = toTimeEntryMapper;
    }

    public async Task<GetTimeEntryForEditOutput> GetForEditAsync(NullableIdDto<int> input)
    {
        var projects = await _projectRepository.GetAllListAsync(p => p.TenantId == AbpSession.TenantId);

        var output = new GetTimeEntryForEditOutput
        {
            ProjectOptions = projects.Select(p => new ComboboxItemDto(p.Id.ToString(), p.Name)).ToList()
        };

        if (input.Id.HasValue)
        {
            var entry = await _timeEntryRepository.GetAsync(input.Id.Value);
            output.TimeEntry = new CreateOrEditTimeEntryDto
            {
                Id = entry.Id,
                ProjectId = entry.ProjectId,
                TaskId = entry.TaskId,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                Description = entry.Description
            };

            var tasks = await _taskRepository.GetAllListAsync(t => t.ProjectId == entry.ProjectId);
            output.TaskOptions = tasks.Select(t => new ComboboxItemDto(t.Id.ToString(), t.Name)).ToList();
        }
        else
        {
            output.TimeEntry = new CreateOrEditTimeEntryDto
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };
        }

        return output;
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_TimeEntries_Create)]
    public async Task<int> CreateAsync(CreateOrEditTimeEntryDto input)
    {
        var entry = _toTimeEntryMapper.Map(input);
        entry.TenantId = AbpSession.TenantId ?? 0;
        entry.UserId = AbpSession.UserId ?? 0;

        var entryId = await _timeEntryRepository.InsertAndGetIdAsync(entry);

        await RecalculateBudgetAsync(input.ProjectId, input.TaskId);

        return entryId;
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_TimeEntries_Edit)]
    public async Task UpdateAsync(CreateOrEditTimeEntryDto input)
    {
        var entry = await _timeEntryRepository.GetAsync(input.Id.Value);
        var oldProjectId = entry.ProjectId;
        var oldTaskId = entry.TaskId;

        _toTimeEntryMapper.Map(input, entry);

        if (oldProjectId != input.ProjectId || oldTaskId != input.TaskId)
        {
            await RecalculateBudgetAsync(oldProjectId, oldTaskId);
        }
        await RecalculateBudgetAsync(input.ProjectId, input.TaskId);
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_TimeEntries_Delete)]
    public async Task DeleteAsync(EntityDto<int> input)
    {
        var entry = await _timeEntryRepository.GetAsync(input.Id);
        var projectId = entry.ProjectId;
        var taskId = entry.TaskId;

        await _timeEntryRepository.DeleteAsync(input.Id);

        await RecalculateBudgetAsync(projectId, taskId);
    }

    [HttpPost]
    public async Task<PagedResultDto<TimeEntryDto>> GetTimeEntriesAsync(GetTimeEntriesInput input)
    {
        var query = _timeEntryRepository.GetAll()
            .Where(e => e.TenantId == AbpSession.TenantId)
            .WhereIf(!string.IsNullOrEmpty(input.Filter), e => e.Description.Contains(input.Filter))
            .WhereIf(input.ProjectId.HasValue, e => e.ProjectId == input.ProjectId.Value)
            .WhereIf(input.TaskId.HasValue, e => e.TaskId == input.TaskId.Value)
            .WhereIf(input.UserId.HasValue, e => e.UserId == input.UserId.Value)
            .WhereIf(input.StartDateFilter.HasValue, e => e.StartTime >= input.StartDateFilter.Value)
            .WhereIf(input.EndDateFilter.HasValue, e => e.StartTime <= input.EndDateFilter.Value);

        var totalCount = await query.CountAsync();
        var entries = await query
            .Include(e => e.Project)
            .Include(e => e.Task)
            .Include(e => e.User)
            .OrderByDescending(e => e.StartTime)
            .PageBy(input)
            .ToListAsync();

        var dtos = entries.Select(e =>
        {
            var dto = _toTimeEntryDtoMapper.Map(e);
            dto.ProjectName = e.Project?.Name;
            dto.ProjectColor = e.Project?.Color;
            dto.TaskName = e.Task?.Name;
            dto.UserName = e.User?.UserName;
            dto.UserFullName = e.User?.FullName;
            dto.DurationHours = e.DurationHours;
            return dto;
        }).ToList();

        return new PagedResultDto<TimeEntryDto>(totalCount, dtos);
    }

    [HttpPost]
    public async Task<List<SchedulerEntryDto>> GetSchedulerEntriesAsync(GetSchedulerEntriesInput input)
    {
        var query = _timeEntryRepository.GetAll()
            .Where(e => e.TenantId == AbpSession.TenantId)
            .WhereIf(input.ForCurrentUserOnly && AbpSession.UserId.HasValue, e => e.UserId == AbpSession.UserId.Value)
            .WhereIf(input.ProjectId.HasValue, e => e.ProjectId == input.ProjectId.Value)
            .WhereIf(input.StartDate.HasValue, e => e.EndTime >= input.StartDate.Value)
            .WhereIf(input.EndDate.HasValue, e => e.StartTime <= input.EndDate.Value);

        var entries = await query
            .Include(e => e.Project)
            .Include(e => e.Task)
            .ToListAsync();

        return entries.Select(e => new SchedulerEntryDto
        {
            Id = e.Id,
            Text = string.IsNullOrEmpty(e.Description) ? $"{e.Project?.Name}" : e.Description,
            StartDate = e.StartTime,
            EndDate = e.EndTime,
            Color = e.Project?.Color ?? "#3498db",
            ProjectId = e.ProjectId,
            ProjectName = e.Project?.Name,
            TaskId = e.TaskId,
            TaskName = e.Task?.Name,
            Description = e.Description
        }).ToList();
    }

    private async Task RecalculateBudgetAsync(int projectId, int? taskId)
    {
        var projectEntries = await _timeEntryRepository.GetAllListAsync(e => e.ProjectId == projectId);
        var usedHours = (decimal)projectEntries.Sum(e => (e.EndTime - e.StartTime).TotalHours);

        var projectBudget = await _projectBudgetRepository.FirstOrDefaultAsync(b => b.ProjectId == projectId);
        if (projectBudget != null)
        {
            projectBudget.UsedHours = usedHours;
            projectBudget.RemainingHours = projectBudget.TotalBudgetHours - usedHours;
        }

        if (taskId.HasValue)
        {
            var taskEntries = await _timeEntryRepository.GetAllListAsync(e => e.TaskId == taskId.Value);
            var taskUsedHours = (decimal)taskEntries.Sum(e => (e.EndTime - e.StartTime).TotalHours);

            var taskBudget = await _taskBudgetRepository.FirstOrDefaultAsync(b => b.TaskId == taskId.Value);
            if (taskBudget != null)
            {
                taskBudget.UsedHours = taskUsedHours;
                taskBudget.RemainingHours = taskBudget.TotalBudgetHours - taskUsedHours;
            }
        }
    }
}
