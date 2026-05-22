using Abp.Mapperly;
using Abp.Organizations;
using CadentManagement.Organizations.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class OrganizationUnitToOrganizationUnitDtoMapper : MapperBase<OrganizationUnit, OrganizationUnitDto>
{
    public override partial OrganizationUnitDto Map(OrganizationUnit source);

    public override partial void Map(OrganizationUnit source, OrganizationUnitDto destination);
}
