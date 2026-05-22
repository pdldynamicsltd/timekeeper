using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.Authorization.Permissions.Dto;

namespace CadentManagement.Authorization.Permissions;

public interface IPermissionAppService : IApplicationService
{
    ListResultDto<FlatPermissionWithLevelDto> GetAllPermissions();
}

