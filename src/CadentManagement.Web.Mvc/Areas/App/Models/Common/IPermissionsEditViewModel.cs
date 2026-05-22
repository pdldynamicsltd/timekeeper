using System.Collections.Generic;
using CadentManagement.Authorization.Permissions.Dto;

namespace CadentManagement.Web.Areas.App.Models.Common;

public interface IPermissionsEditViewModel
{
    List<FlatPermissionDto> Permissions { get; set; }

    List<string> GrantedPermissionNames { get; set; }
}

