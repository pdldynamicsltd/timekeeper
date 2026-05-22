using Abp.Mapperly;
using Abp.Organizations;
using CadentManagement.Organizations.Dto;
using CadentManagement.Web.Areas.App.Models.OrganizationUnits;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class OrganizationUnitDtoToEditOrganizationUnitModalViewModelMapper : MapperBase<OrganizationUnitDto, EditOrganizationUnitModalViewModel>
{
    public override partial EditOrganizationUnitModalViewModel Map(OrganizationUnitDto source);

    public override partial void Map(OrganizationUnitDto source, EditOrganizationUnitModalViewModel destination);
}

[Mapper]
public partial class OrganizationUnitToEditOrganizationUnitModalViewModelMapper : MapperBase<OrganizationUnit, EditOrganizationUnitModalViewModel>
{
    public override partial EditOrganizationUnitModalViewModel Map(OrganizationUnit source);

    public override partial void Map(OrganizationUnit source, EditOrganizationUnitModalViewModel destination);
}
