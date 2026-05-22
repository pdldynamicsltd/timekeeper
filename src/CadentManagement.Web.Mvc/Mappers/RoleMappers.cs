using Abp.Mapperly;
using CadentManagement.Authorization.Roles.Dto;
using CadentManagement.Web.Areas.App.Models.Roles;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class GetRoleForEditOutputToCreateOrEditRoleModalViewModelMapper : MapperBase<GetRoleForEditOutput, CreateOrEditRoleModalViewModel>
{
    public override partial CreateOrEditRoleModalViewModel Map(GetRoleForEditOutput source);
    public override partial void Map(GetRoleForEditOutput source, CreateOrEditRoleModalViewModel destination);
}
