using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.TimeTracking.Tasks.Dto;

namespace CadentManagement.TimeTracking.Tasks;

public interface IProjectTaskAppService : IApplicationService
{
    Task<GetTaskForEditOutput> GetForEditAsync(NullableIdDto<int> input);

    Task<int> CreateAsync(CreateOrEditTaskDto input);

    Task UpdateAsync(CreateOrEditTaskDto input);

    Task DeleteAsync(EntityDto<int> input);

    Task<PagedResultDto<TaskDto>> GetTasksAsync(GetTasksInput input);

    Task<List<TaskDto>> GetProjectTaskTreeAsync(EntityDto<int> input);

    Task<List<TaskDto>> GetTasksForProjectAsync(int projectId);
}
