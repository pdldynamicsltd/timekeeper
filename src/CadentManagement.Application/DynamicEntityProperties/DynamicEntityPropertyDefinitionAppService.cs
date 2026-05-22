using System.Collections.Generic;
using Abp.Authorization;
using Abp.DynamicEntityProperties;
using CadentManagement.Authorization;

namespace CadentManagement.DynamicEntityProperties;

[AbpAuthorize(AppPermissions.Pages_Administration_DynamicProperties)]
public class DynamicEntityPropertyDefinitionAppService : CadentManagementAppServiceBase, IDynamicEntityPropertyDefinitionAppService
{
    private readonly IDynamicEntityPropertyDefinitionManager _dynamicEntityPropertyDefinitionManager;

    public DynamicEntityPropertyDefinitionAppService(IDynamicEntityPropertyDefinitionManager dynamicEntityPropertyDefinitionManager)
    {
        _dynamicEntityPropertyDefinitionManager = dynamicEntityPropertyDefinitionManager;
    }

    public List<string> GetAllAllowedInputTypeNames()
    {
        return _dynamicEntityPropertyDefinitionManager.GetAllAllowedInputTypeNames();
    }

    public List<string> GetAllEntities()
    {
        return _dynamicEntityPropertyDefinitionManager.GetAllEntities();
    }
}
