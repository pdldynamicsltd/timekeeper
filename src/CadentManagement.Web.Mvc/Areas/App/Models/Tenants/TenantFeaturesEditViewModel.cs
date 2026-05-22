using CadentManagement.MultiTenancy;
using CadentManagement.MultiTenancy.Dto;
using CadentManagement.Web.Areas.App.Models.Common;

namespace CadentManagement.Web.Areas.App.Models.Tenants;

public class TenantFeaturesEditViewModel : GetTenantFeaturesEditOutput, IFeatureEditViewModel
{
    public Tenant Tenant { get; set; }
}

