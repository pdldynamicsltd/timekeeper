using Abp.Mapperly;
using CadentManagement.UserTasks;
using CadentManagement.UserTasks.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class UserTaskToUserTaskDtoMapper : MapperBase<UserTask, UserTaskDto>
{
    public override partial UserTaskDto Map(UserTask source);

    public override partial void Map(UserTask source, UserTaskDto destination);
}

[Mapper]
public partial class CreateOrEditUserTaskDtoToUserTaskMapper : MapperBase<CreateOrEditUserTaskDto, UserTask>
{
    public override partial UserTask Map(CreateOrEditUserTaskDto source);

    public override partial void Map(CreateOrEditUserTaskDto source, UserTask destination);
}
