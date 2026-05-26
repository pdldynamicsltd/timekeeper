using Abp.Mapperly;
using CadentManagement.TimeTracking.Tasks;
using CadentManagement.TimeTracking.Tasks.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class ProjectTaskToTaskDtoMapper : MapperBase<ProjectTask, TaskDto>
{
    public override partial TaskDto Map(ProjectTask source);

    public override partial void Map(ProjectTask source, TaskDto destination);
}

[Mapper]
public partial class CreateOrEditTaskDtoToProjectTaskMapper : MapperBase<CreateOrEditTaskDto, ProjectTask>
{
    public override partial ProjectTask Map(CreateOrEditTaskDto source);

    public override partial void Map(CreateOrEditTaskDto source, ProjectTask destination);
}
