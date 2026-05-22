using System.Linq;
using Abp.Authorization.Users;
using CadentManagement.Authorization.Users.Dto;
using CadentManagement.Security;
using CadentManagement.Web.Areas.App.Models.Common;

namespace CadentManagement.Web.Areas.App.Models.Users;

public class CreateOrEditUserModalViewModel : GetUserForEditOutput, IOrganizationUnitsEditViewModel
{
    public bool CanChangeUserName => User.UserName != AbpUserBase.AdminUserName;

    public int AssignedRoleCount
    {
        get { return Roles.Count(r => r.IsAssigned); }
    }

    public int AssignedOrganizationUnitCount => MemberedOrganizationUnits.Count;

    public bool IsEditMode => User.Id.HasValue;

    public PasswordComplexitySetting PasswordComplexitySetting { get; set; }
}

