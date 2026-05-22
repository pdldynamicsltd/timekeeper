using Abp.Authorization;
using CadentManagement.Authorization.Roles;
using CadentManagement.Authorization.Users;

namespace CadentManagement.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {

    }
}

