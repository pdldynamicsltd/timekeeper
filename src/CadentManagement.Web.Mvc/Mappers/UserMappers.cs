using Abp.Mapperly;
using CadentManagement.Authorization.Users.Dto;
using CadentManagement.Web.Areas.App.Models.Users;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class GetUserForEditOutputToCreateOrEditUserModalViewModelMapper : MapperBase<GetUserForEditOutput, CreateOrEditUserModalViewModel>
{
    public override partial CreateOrEditUserModalViewModel Map(GetUserForEditOutput source);

    public override partial void Map(GetUserForEditOutput source, CreateOrEditUserModalViewModel destination);
}

[Mapper]
public partial class GetUserPermissionsForEditOutputToUserPermissionsEditViewModelMapper : MapperBase<GetUserPermissionsForEditOutput, UserPermissionsEditViewModel>
{
    public override partial UserPermissionsEditViewModel Map(GetUserPermissionsForEditOutput source);

    public override partial void Map(GetUserPermissionsForEditOutput source, UserPermissionsEditViewModel destination);
}
