using System.Collections.Generic;
using Abp.Application.Services.Dto;
using CadentManagement.Authorization.Permissions.Dto;
using CadentManagement.Web.Areas.App.Models.Common;

namespace CadentManagement.Web.Areas.App.Models.Roles;

public class RoleListViewModel : IPermissionsEditViewModel
{
    public List<FlatPermissionDto> Permissions { get; set; }

    public List<string> GrantedPermissionNames { get; set; }
}

