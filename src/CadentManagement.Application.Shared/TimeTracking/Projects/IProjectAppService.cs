using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.TimeTracking.Projects.Dto;

namespace CadentManagement.TimeTracking.Projects;

public interface IProjectAppService : IApplicationService
{
    Task<GetProjectForEditOutput> GetForEditAsync(NullableIdDto<int> input);

    Task<int> CreateAsync(CreateOrEditProjectDto input);

    Task UpdateAsync(CreateOrEditProjectDto input);

    Task DeleteAsync(EntityDto<int> input);

    Task<PagedResultDto<ProjectDto>> GetProjectsAsync(GetProjectsInput input);

    Task<ProjectBudgetSummaryDto> GetProjectBudgetSummaryAsync(EntityDto<int> input);
}
