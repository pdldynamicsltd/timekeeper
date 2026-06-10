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
using CadentManagement.Dto;
using CadentManagement.Mappers;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.Reports.Dto;
using CadentManagement.TimeTracking.Reports.Exporting;
using CadentManagement.TimeTracking.Tasks;
using CadentManagement.TimeTracking.TimeEntries;
using CadentManagement.TimeTracking.TimeEntries.Dto;

namespace CadentManagement.TimeTracking.Reports;

[AbpAuthorize(AppPermissions.Pages_TimeTracking_Reports)]
public class ReportingAppService : CadentManagementAppServiceBase, IReportingAppService
{
    private readonly IRepository<TimeEntry, int> _timeEntryRepository;
    private readonly IRepository<Project, int> _projectRepository;
    private readonly IRepository<ProjectTask, int> _taskRepository;
    private readonly IRepository<ProjectBudgetTracking, int> _projectBudgetRepository;
    private readonly IRepository<TaskBudgetTracking, int> _taskBudgetRepository;
    private readonly ITimeEntryListExcelExporter _excelExporter;
    private readonly TimeEntryToTimeEntryDtoMapper _toTimeEntryDtoMapper;

    public ReportingAppService(
        IRepository<TimeEntry, int> timeEntryRepository,
        IRepository<Project, int> projectRepository,
        IRepository<ProjectTask, int> taskRepository,
        IRepository<ProjectBudgetTracking, int> projectBudgetRepository,
        IRepository<TaskBudgetTracking, int> taskBudgetRepository,
        ITimeEntryListExcelExporter excelExporter,
        TimeEntryToTimeEntryDtoMapper toTimeEntryDtoMapper)
    {
        _timeEntryRepository = timeEntryRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _projectBudgetRepository = projectBudgetRepository;
        _taskBudgetRepository = taskBudgetRepository;
        _excelExporter = excelExporter;
        _toTimeEntryDtoMapper = toTimeEntryDtoMapper;
    }

    public async Task<ProjectBudgetReportDto> GetProjectBudgetReportAsync(EntityDto<int> input)
    {
        var project = await _projectRepository.GetAsync(input.Id);
        var projectBudget = await _projectBudgetRepository.FirstOrDefaultAsync(b => b.ProjectId == input.Id);
        var taskBudgets = await _taskBudgetRepository.GetAll()
            .Include(b => b.Task)
            .Where(b => b.Task.ProjectId == input.Id)
            .ToListAsync();

        return new ProjectBudgetReportDto
        {
            BudgetSummary = new Projects.Dto.ProjectBudgetSummaryDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                BudgetType = project.BudgetType,
                TotalBudgetHours = projectBudget?.TotalBudgetHours ?? 0,
                UsedHours = projectBudget?.UsedHours ?? 0,
                RemainingHours = projectBudget?.RemainingHours ?? 0,
                UtilizationPercentage = projectBudget?.UtilizationPercentage ?? 0
            },
            TaskBudgets = taskBudgets.Select(b => new TaskBudgetSummaryDto
            {
                TaskId = b.TaskId,
                TaskName = b.Task?.Name,
                ParentTaskId = b.Task?.ParentTaskId,
                TotalBudgetHours = b.TotalBudgetHours,
                UsedHours = b.UsedHours,
                RemainingHours = b.RemainingHours,
                UtilizationPercentage = b.UtilizationPercentage
            }).ToList()
        };
    }

    [HttpPost]
    public async Task<PagedResultDto<TimeEntryDto>> GetTimeEntriesReportAsync(GetTimeEntriesReportInput input)
    {
        var query = BuildReportQuery(input);

        var totalCount = await query.CountAsync();
        var entries = await query
            .Include(e => e.Project)
            .Include(e => e.Task)
            .Include(e => e.User)
            .OrderByDescending(e => e.StartTime)
            .ToListAsync();

        var dtos = MapTimeEntries(entries);
        return new PagedResultDto<TimeEntryDto>(totalCount, dtos);
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_Reports_Export)]
    [HttpPost]
    public async Task<FileDto> ExportToExcelAsync(GetTimeEntriesReportInput input)
    {
        var query = BuildReportQuery(input);
        var entries = await query
            .Include(e => e.Project)
            .Include(e => e.Task)
            .Include(e => e.User)
            .OrderByDescending(e => e.StartTime)
            .ToListAsync();

        var dtos = MapTimeEntries(entries);
        return await _excelExporter.ExportToFileAsync(dtos);
    }

    private IQueryable<TimeEntry> BuildReportQuery(GetTimeEntriesReportInput input)
    {
        return _timeEntryRepository.GetAll()
            .Where(e => e.TenantId == AbpSession.TenantId)
            .WhereIf(input.ProjectId.HasValue, e => e.ProjectId == input.ProjectId.Value)
            .WhereIf(input.TaskId.HasValue, e => e.TaskId == input.TaskId.Value)
            .WhereIf(input.UserId.HasValue, e => e.UserId == input.UserId.Value)
            .WhereIf(input.StartDate.HasValue, e => e.StartTime >= input.StartDate.Value)
            .WhereIf(input.EndDate.HasValue, e => e.StartTime <= input.EndDate.Value);
    }

    private List<TimeEntryDto> MapTimeEntries(List<TimeEntry> entries)
    {
        return entries.Select(e =>
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
    }
}
