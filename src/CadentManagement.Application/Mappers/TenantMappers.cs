using Abp.Application.Editions;
using Abp.Mapperly;
using CadentManagement.Authorization.Accounts.Dto;
using CadentManagement.MultiTenancy;
using CadentManagement.MultiTenancy.Dto;
using CadentManagement.MultiTenancy.HostDashboard.Dto;
using CadentManagement.Sessions.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class TenantToRecentTenantMapper : MapperBase<Tenant, RecentTenant>
{
    public override partial RecentTenant Map(Tenant source);

    public override partial void Map(Tenant source, RecentTenant destination);
}

[Mapper]
public partial class TenantToTenantLoginInfoDtoMapper(
    EditionToEditionInfoDtoMapper editionMapper) : MapperBase<Tenant, TenantLoginInfoDto>
{
    public override partial TenantLoginInfoDto Map(Tenant source);

    public override partial void Map(Tenant source, TenantLoginInfoDto destination);

    private EditionInfoDto? MapEdition(Edition? source)
    {
        return source is null ? null : editionMapper.Map(source);
    }
}

[Mapper]
public partial class TenantToTenantListDtoMapper : MapperBase<Tenant, TenantListDto>
{
    public override partial TenantListDto Map(Tenant source);

    public override partial void Map(Tenant source, TenantListDto destination);
}

[Mapper]
public partial class TenantToTenantEditDtoMapper : MapperBase<Tenant, TenantEditDto>
{
    public override partial TenantEditDto Map(Tenant source);

    public override partial void Map(Tenant source, TenantEditDto destination);
}

[Mapper]
public partial class TenantEditDtoToTenantMapper : MapperBase<TenantEditDto, Tenant>
{
    public override partial Tenant Map(TenantEditDto source);

    public override partial void Map(TenantEditDto source, Tenant destination);
}

[Mapper]
public partial class TenantToCurrentTenantInfoDtoMapper : MapperBase<Tenant, CurrentTenantInfoDto>
{
    public override partial CurrentTenantInfoDto Map(Tenant source);

    public override partial void Map(Tenant source, CurrentTenantInfoDto destination);
}

[Mapper]
public partial class CurrentTenantInfoDtoToTenantMapper : MapperBase<CurrentTenantInfoDto, Tenant>
{
    public override partial Tenant Map(CurrentTenantInfoDto source);

    public override partial void Map(CurrentTenantInfoDto source, Tenant destination);
}
