using Abp.AspNetCore.Mvc.ViewComponents;

namespace CadentManagement.Web.Views;

public abstract class CadentManagementViewComponent : AbpViewComponent
{
    protected CadentManagementViewComponent()
    {
        LocalizationSourceName = CadentManagementConsts.LocalizationSourceName;
    }
}

