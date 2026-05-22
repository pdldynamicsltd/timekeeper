using Abp.Domain.Services;

namespace CadentManagement;

public abstract class CadentManagementDomainServiceBase : DomainService
{
    /* Add your common members for all your domain services. */

    protected CadentManagementDomainServiceBase()
    {
        LocalizationSourceName = CadentManagementConsts.LocalizationSourceName;
    }
}

