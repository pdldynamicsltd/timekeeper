using Abp.Mapperly;
using Abp.Organizations;
using CadentManagement.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.GraphQL.Mappers;

[Mapper]
public partial class OrganizationUnitToOrganizationUnitDtoMapper : MapperBase<OrganizationUnit, OrganizationUnitDto>
{
    public override partial OrganizationUnitDto Map(OrganizationUnit source);

    public override partial void Map(OrganizationUnit source, OrganizationUnitDto destination);
}

[Mapper]
public partial class OrganizationUnitToUserDtoOrganizationUnitDtoMapper : MapperBase<OrganizationUnit, UserDto.OrganizationUnitDto>
{
    public override partial UserDto.OrganizationUnitDto Map(OrganizationUnit source);

    public override partial void Map(OrganizationUnit source, UserDto.OrganizationUnitDto destination);
}
