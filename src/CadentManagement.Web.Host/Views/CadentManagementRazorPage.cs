using Abp.AspNetCore.Mvc.Views;

namespace CadentManagement.Web.Views;

public abstract class CadentManagementRazorPage<TModel> : AbpRazorPage<TModel>
{
    protected CadentManagementRazorPage()
    {
        LocalizationSourceName = CadentManagementConsts.LocalizationSourceName;
    }
}

