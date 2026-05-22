using CadentManagement.Authorization.Users;
using CadentManagement.Authorization.Users.Dto;
using CadentManagement.Web.Areas.App.Models.Common;

namespace CadentManagement.Web.Areas.App.Models.Users;

public class UserPermissionsEditViewModel : GetUserPermissionsForEditOutput, IPermissionsEditViewModel
{
    public User User { get; set; }
}

