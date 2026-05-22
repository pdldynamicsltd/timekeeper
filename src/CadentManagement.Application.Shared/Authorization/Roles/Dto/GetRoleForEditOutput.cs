using System.Collections.Generic;
using CadentManagement.Authorization.Permissions.Dto;

namespace CadentManagement.Authorization.Roles.Dto;

public class GetRoleForEditOutput
{
    public RoleEditDto Role { get; set; }

    public List<FlatPermissionDto> Permissions { get; set; }

    public List<string> GrantedPermissionNames { get; set; }
}

