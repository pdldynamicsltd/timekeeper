using Abp.Mapperly;
using CadentManagement.Maui.Models.Common;
using CadentManagement.Sessions.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Maui.Mappers;

public class GetCurrentLoginInformationsOutputToCurrentLoginInformationPersistanceModelMapper : MapperBase<GetCurrentLoginInformationsOutput, CurrentLoginInformationPersistanceModel>
{
    private readonly UserLoginInfoDtoToUserLoginInfoPersistanceModelMapper _userMapper = new();
    private readonly TenantLoginInfoDtoToTenantLoginInfoPersistanceModelMapper _tenantMapper = new();
    private readonly ApplicationInfoDtoToApplicationInfoPersistanceModelMapper _applicationMapper = new();

    public override CurrentLoginInformationPersistanceModel Map(GetCurrentLoginInformationsOutput source)
    {
        return new CurrentLoginInformationPersistanceModel
        {
            User = source.User != null ? _userMapper.Map(source.User) : null,
            Tenant = source.Tenant != null ? _tenantMapper.Map(source.Tenant) : null,
            Application = source.Application != null ? _applicationMapper.Map(source.Application) : null
        };
    }

    public override void Map(GetCurrentLoginInformationsOutput source, CurrentLoginInformationPersistanceModel destination)
    {
        destination.User = source.User != null ? _userMapper.Map(source.User) : null;
        destination.Tenant = source.Tenant != null ? _tenantMapper.Map(source.Tenant) : null;
        destination.Application = source.Application != null ? _applicationMapper.Map(source.Application) : null;
    }
}

public class CurrentLoginInformationPersistanceModelToGetCurrentLoginInformationsOutputMapper : MapperBase<CurrentLoginInformationPersistanceModel, GetCurrentLoginInformationsOutput>
{
    private readonly UserLoginInfoPersistanceModelToUserLoginInfoDtoMapper _userMapper = new();
    private readonly TenantLoginInfoPersistanceModelToTenantLoginInfoDtoMapper _tenantMapper = new();
    private readonly ApplicationInfoPersistanceModelToApplicationInfoDtoMapper _applicationMapper = new();

    public override GetCurrentLoginInformationsOutput Map(CurrentLoginInformationPersistanceModel source)
    {
        return new GetCurrentLoginInformationsOutput
        {
            User = source.User != null ? _userMapper.Map(source.User) : null,
            Tenant = source.Tenant != null ? _tenantMapper.Map(source.Tenant) : null,
            Application = source.Application != null ? _applicationMapper.Map(source.Application) : null
        };
    }

    public override void Map(CurrentLoginInformationPersistanceModel source, GetCurrentLoginInformationsOutput destination)
    {
        destination.User = source.User != null ? _userMapper.Map(source.User) : null;
        destination.Tenant = source.Tenant != null ? _tenantMapper.Map(source.Tenant) : null;
        destination.Application = source.Application != null ? _applicationMapper.Map(source.Application) : null;
    }
}

[Mapper]
public partial class UserLoginInfoDtoToUserLoginInfoPersistanceModelMapper : MapperBase<UserLoginInfoDto, UserLoginInfoPersistanceModel>
{
    public override partial UserLoginInfoPersistanceModel Map(UserLoginInfoDto source);

    public override partial void Map(UserLoginInfoDto source, UserLoginInfoPersistanceModel destination);
}

[Mapper]
public partial class UserLoginInfoPersistanceModelToUserLoginInfoDtoMapper : MapperBase<UserLoginInfoPersistanceModel, UserLoginInfoDto>
{
    public override partial UserLoginInfoDto Map(UserLoginInfoPersistanceModel source);

    public override partial void Map(UserLoginInfoPersistanceModel source, UserLoginInfoDto destination);
}

[Mapper]
public partial class TenantLoginInfoDtoToTenantLoginInfoPersistanceModelMapper : MapperBase<TenantLoginInfoDto, TenantLoginInfoPersistanceModel>
{
    public override partial TenantLoginInfoPersistanceModel Map(TenantLoginInfoDto source);

    public override partial void Map(TenantLoginInfoDto source, TenantLoginInfoPersistanceModel destination);
}

[Mapper]
public partial class TenantLoginInfoPersistanceModelToTenantLoginInfoDtoMapper : MapperBase<TenantLoginInfoPersistanceModel, TenantLoginInfoDto>
{
    public override partial TenantLoginInfoDto Map(TenantLoginInfoPersistanceModel source);

    public override partial void Map(TenantLoginInfoPersistanceModel source, TenantLoginInfoDto destination);
}

[Mapper]
public partial class ApplicationInfoDtoToApplicationInfoPersistanceModelMapper : MapperBase<ApplicationInfoDto, ApplicationInfoPersistanceModel>
{
    public override partial ApplicationInfoPersistanceModel Map(ApplicationInfoDto source);

    public override partial void Map(ApplicationInfoDto source, ApplicationInfoPersistanceModel destination);
}

[Mapper]
public partial class ApplicationInfoPersistanceModelToApplicationInfoDtoMapper : MapperBase<ApplicationInfoPersistanceModel, ApplicationInfoDto>
{
    public override partial ApplicationInfoDto Map(ApplicationInfoPersistanceModel source);

    public override partial void Map(ApplicationInfoPersistanceModel source, ApplicationInfoDto destination);
}
