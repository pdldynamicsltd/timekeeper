using CadentManagement.Authorization.Roles.Dto;
using CadentManagement.Web.Areas.App.Models.Common;

namespace CadentManagement.Web.Areas.App.Models.Roles;

public class CreateOrEditRoleModalViewModel : GetRoleForEditOutput, IPermissionsEditViewModel
{
    public bool IsEditMode => Role.Id.HasValue;
}

