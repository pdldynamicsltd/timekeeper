using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.UserTasks.Dto;

namespace CadentManagement.UserTasks;

public interface IUserTaskAppService : IApplicationService
{
    Task<GetUserTaskForEditOutput> GetForEditAsync(NullableIdDto<int> input);

    Task<PagedResultDto<UserTaskDto>> GetTasksAsync(GetUserTasksInput input);

    Task<int> CreateAsync(CreateOrEditUserTaskDto input);

    Task UpdateAsync(CreateOrEditUserTaskDto input);

    Task DeleteAsync(EntityDto<int> input);

    Task UpdateStatusAsync(UpdateTaskStatusInput input);

    Task UpdateDueDateAsync(UpdateTaskDueDateInput input);

    Task CompleteAsync(EntityDto<int> input);

    Task ConvertToTimeEntryAsync(ConvertTaskToTimeEntryInput input);

    Task<List<TodoStatusDto>> GetTodoStatusesAsync();

    Task<int> CreateTodoStatusAsync(CreateOrEditTodoStatusDto input);

    Task UpdateTodoStatusAsync(CreateOrEditTodoStatusDto input);

    Task DeleteTodoStatusAsync(EntityDto<int> input);
}