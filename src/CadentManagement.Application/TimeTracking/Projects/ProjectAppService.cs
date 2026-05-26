using System;
using System.Collections.Generic;
using System.Globalization;
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
using CadentManagement.TimeTracking.Dto;
using CadentManagement.TimeTracking.Importing;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.Projects.Dto;

namespace CadentManagement.TimeTracking.Projects;

[AbpAuthorize(AppPermissions.Pages_TimeTracking_Projects)]
public class ProjectAppService : CadentManagementAppServiceBase, IProjectAppService
{
    private readonly IRepository<Project, int> _projectRepository;
    private readonly IRepository<ProjectBudgetTracking, int> _budgetTrackingRepository;
    private readonly ProjectToProjectDtoMapper _toProjectDtoMapper;
    private readonly CreateOrEditProjectDtoToProjectMapper _toProjectMapper;

    public ProjectAppService(
        IRepository<Project, int> projectRepository,
        IRepository<ProjectBudgetTracking, int> budgetTrackingRepository,
        ProjectToProjectDtoMapper toProjectDtoMapper,
        CreateOrEditProjectDtoToProjectMapper toProjectMapper)
    {
        _projectRepository = projectRepository;
        _budgetTrackingRepository = budgetTrackingRepository;
        _toProjectDtoMapper = toProjectDtoMapper;
        _toProjectMapper = toProjectMapper;
    }

    public async Task<GetProjectForEditOutput> GetForEditAsync(NullableIdDto<int> input)
    {
        var output = new GetProjectForEditOutput
        {
            BudgetTypeOptions = GetBudgetTypeOptions(),
            StatusOptions = GetStatusOptions()
        };

        if (input.Id.HasValue)
        {
            var project = await _projectRepository.GetAsync(input.Id.Value);
            output.Project = new CreateOrEditProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                BudgetType = project.BudgetType,
                BudgetHours = project.BudgetHours,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                IsPublic = project.IsPublic,
                Color = project.Color
            };
        }
        else
        {
            output.Project = new CreateOrEditProjectDto
            {
                Status = ProjectStatus.Active,
                BudgetType = BudgetType.NoBudget,
                Color = "#3498db"
            };
        }

        return output;
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_Projects_Create)]
    public async Task<int> CreateAsync(CreateOrEditProjectDto input)
    {
        var project = _toProjectMapper.Map(input);
        project.TenantId = AbpSession.TenantId ?? 0;

        var projectId = await _projectRepository.InsertAndGetIdAsync(project);

        await CreateBudgetTrackingAsync(projectId, project.BudgetHours);

        return projectId;
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_Projects_Edit)]
    public async Task UpdateAsync(CreateOrEditProjectDto input)
    {
        var project = await _projectRepository.GetAsync(input.Id.Value);
        _toProjectMapper.Map(input, project);

        await UpdateBudgetHoursAsync(project.Id, project.BudgetHours);
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_Projects_Delete)]
    public async Task DeleteAsync(EntityDto<int> input)
    {
        await _projectRepository.DeleteAsync(input.Id);
    }

    [HttpPost]
    public async Task<PagedResultDto<ProjectDto>> GetProjectsAsync(GetProjectsInput input)
    {
        var query = _projectRepository.GetAll()
            .Where(p => p.TenantId == AbpSession.TenantId)
            .WhereIf(!string.IsNullOrEmpty(input.Filter), p => p.Name.Contains(input.Filter))
            .WhereIf(input.StatusFilter.HasValue, p => p.Status == input.StatusFilter.Value);

        var totalCount = await query.CountAsync();
        var projects = await query
            .Include(p => p.BudgetTracking)
            .OrderBy(p => p.Name)
            .PageBy(input)
            .ToListAsync();

        var projectDtos = projects.Select(p =>
        {
            var dto = _toProjectDtoMapper.Map(p);
            if (p.BudgetTracking != null)
            {
                dto.UsedHours = p.BudgetTracking.UsedHours;
                dto.RemainingHours = p.BudgetTracking.RemainingHours;
                dto.UtilizationPercentage = p.BudgetTracking.UtilizationPercentage;
            }
            return dto;
        }).ToList();

        return new PagedResultDto<ProjectDto>(totalCount, projectDtos);
    }

    public async Task<ProjectBudgetSummaryDto> GetProjectBudgetSummaryAsync(EntityDto<int> input)
    {
        var project = await _projectRepository.GetAsync(input.Id);
        var budgetTracking = await _budgetTrackingRepository.FirstOrDefaultAsync(b => b.ProjectId == input.Id);

        return new ProjectBudgetSummaryDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            BudgetType = project.BudgetType,
            TotalBudgetHours = budgetTracking?.TotalBudgetHours ?? 0,
            UsedHours = budgetTracking?.UsedHours ?? 0,
            RemainingHours = budgetTracking?.RemainingHours ?? 0,
            UtilizationPercentage = budgetTracking?.UtilizationPercentage ?? 0
        };
    }

    [AbpAuthorize(AppPermissions.Pages_TimeTracking_Projects_Create)]
    [HttpPost]
    public async Task<ImportCsvResultDto> ImportProjectsFromCsvAsync(ImportProjectsCsvInput input)
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var rows = CsvImportParser.Parse(input.CsvContent);
        var result = new ImportCsvResultDto();

        foreach (var row in rows)
        {
            var projectName = GetValue(row, "Project");
            if (string.IsNullOrWhiteSpace(projectName))
            {
                result.SkippedRows++;
                continue;
            }

            var estimatedHours = ParseDecimal(GetValue(row, "Estimated (h)"));
            var status = ParseStatus(GetValue(row, "Status"));
            var visibility = GetValue(row, "Visibility");
            var note = GetValue(row, "Note");
            var client = GetValue(row, "Client");
            var description = string.IsNullOrWhiteSpace(note) ? client : note;

            var project = await _projectRepository.FirstOrDefaultAsync(p =>
                p.TenantId == tenantId && p.Name == projectName);

            if (project == null)
            {
                project = new Project
                {
                    TenantId = tenantId,
                    Name = projectName,
                    Description = description,
                    Status = status,
                    IsPublic = string.Equals(visibility, "Public", StringComparison.OrdinalIgnoreCase),
                    BudgetHours = estimatedHours,
                    BudgetType = estimatedHours > 0 ? BudgetType.ProjectBudget : BudgetType.NoBudget,
                    Color = "#3498db"
                };

                var projectId = await _projectRepository.InsertAndGetIdAsync(project);
                await CreateBudgetTrackingAsync(projectId, estimatedHours);
                result.CreatedProjects++;
                continue;
            }

            project.Description = string.IsNullOrWhiteSpace(project.Description) ? description : project.Description;
            project.Status = status;
            project.IsPublic = string.Equals(visibility, "Public", StringComparison.OrdinalIgnoreCase);
            project.BudgetHours = estimatedHours;
            project.BudgetType = estimatedHours > 0 ? BudgetType.ProjectBudget : BudgetType.NoBudget;

            await UpdateBudgetHoursAsync(project.Id, estimatedHours);
            result.UpdatedProjects++;
        }

        return result;
    }

    private async Task CreateBudgetTrackingAsync(int projectId, decimal budgetHours)
    {
        var tracking = new ProjectBudgetTracking
        {
            ProjectId = projectId,
            TotalBudgetHours = budgetHours,
            UsedHours = 0,
            RemainingHours = budgetHours
        };
        await _budgetTrackingRepository.InsertAsync(tracking);
    }

    private async Task UpdateBudgetHoursAsync(int projectId, decimal newBudgetHours)
    {
        var tracking = await _budgetTrackingRepository.FirstOrDefaultAsync(b => b.ProjectId == projectId);
        if (tracking != null)
        {
            tracking.TotalBudgetHours = newBudgetHours;
            tracking.RemainingHours = newBudgetHours - tracking.UsedHours;
        }
        else
        {
            await CreateBudgetTrackingAsync(projectId, newBudgetHours);
        }
    }

    private static string GetValue(Dictionary<string, string> row, string key)
    {
        return row.TryGetValue(key, out var value) ? value?.Trim() : null;
    }

    private static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static ProjectStatus ParseStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ProjectStatus.Active;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "completed" => ProjectStatus.Completed,
            "archived" => ProjectStatus.Archived,
            _ => ProjectStatus.Active
        };
    }

    private List<ComboboxItemDto> GetBudgetTypeOptions()
    {
        return new List<ComboboxItemDto>
        {
            new ComboboxItemDto(((int)BudgetType.NoBudget).ToString(), L("NoBudget")),
            new ComboboxItemDto(((int)BudgetType.ProjectBudget).ToString(), L("ProjectBudget")),
            new ComboboxItemDto(((int)BudgetType.TaskBudget).ToString(), L("TaskBudget"))
        };
    }

    private List<ComboboxItemDto> GetStatusOptions()
    {
        return new List<ComboboxItemDto>
        {
            new ComboboxItemDto(((int)ProjectStatus.Active).ToString(), L("ActiveStatus")),
            new ComboboxItemDto(((int)ProjectStatus.Archived).ToString(), L("ArchivedStatus")),
            new ComboboxItemDto(((int)ProjectStatus.Completed).ToString(), L("CompletedStatus"))
        };
    }
}
