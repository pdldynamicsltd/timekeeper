using Abp.Mapperly;
using CadentManagement.MultiTenancy.Dto;
using CadentManagement.Web.Areas.App.Models.Tenants;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class GetTenantFeaturesEditOutputToTenantFeaturesEditViewModelMapper : MapperBase<GetTenantFeaturesEditOutput, TenantFeaturesEditViewModel>
{
    public override partial TenantFeaturesEditViewModel Map(GetTenantFeaturesEditOutput source);

    public override partial void Map(GetTenantFeaturesEditOutput source, TenantFeaturesEditViewModel destination);
}
