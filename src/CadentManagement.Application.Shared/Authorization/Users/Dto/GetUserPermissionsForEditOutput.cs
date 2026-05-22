using System.Collections.Generic;
using CadentManagement.Authorization.Permissions.Dto;

namespace CadentManagement.Authorization.Users.Dto;

public class GetUserPermissionsForEditOutput
{
    public List<FlatPermissionDto> Permissions { get; set; }

    public List<string> GrantedPermissionNames { get; set; }
}

