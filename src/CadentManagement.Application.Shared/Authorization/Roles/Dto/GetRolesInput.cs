using System.Collections.Generic;

namespace CadentManagement.Authorization.Roles.Dto;

public class GetRolesInput
{
    public List<string> Permissions { get; set; }
}

