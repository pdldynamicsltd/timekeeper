using Abp.Mapperly;
using CadentManagement.MultiTenancy.Dto;
using CadentManagement.Web.Models.TenantRegistration;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class EditionsSelectOutputToEditionsSelectViewModelMapper : MapperBase<EditionsSelectOutput, EditionsSelectViewModel>
{
    public override partial EditionsSelectViewModel Map(EditionsSelectOutput source);
    public override partial void Map(EditionsSelectOutput source, EditionsSelectViewModel destination);
}

[Mapper]
public partial class RegisterTenantOutputToTenantRegisterResultViewModelMapper : MapperBase<RegisterTenantOutput, TenantRegisterResultViewModel>
{
    public override partial TenantRegisterResultViewModel Map(RegisterTenantOutput source);

    public override partial void Map(RegisterTenantOutput source, TenantRegisterResultViewModel destination);
}
