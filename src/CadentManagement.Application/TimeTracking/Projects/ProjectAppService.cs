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
