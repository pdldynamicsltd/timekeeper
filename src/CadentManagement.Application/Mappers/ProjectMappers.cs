using Abp.Mapperly;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.Projects.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class ProjectToProjectDtoMapper : MapperBase<Project, ProjectDto>
{
    public override partial ProjectDto Map(Project source);

    public override partial void Map(Project source, ProjectDto destination);
}

[Mapper]
public partial class CreateOrEditProjectDtoToProjectMapper : MapperBase<CreateOrEditProjectDto, Project>
{
    public override partial Project Map(CreateOrEditProjectDto source);

    public override partial void Map(CreateOrEditProjectDto source, Project destination);
}
