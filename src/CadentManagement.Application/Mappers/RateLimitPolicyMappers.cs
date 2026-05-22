using Abp.Mapperly;
using CadentManagement.RateLimiting;
using CadentManagement.RateLimiting.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class RateLimitPolicyToRateLimitPolicyDtoMapper : MapperBase<RateLimitPolicy, RateLimitPolicyDto>
{
    public override partial RateLimitPolicyDto Map(RateLimitPolicy source);

    public override partial void Map(RateLimitPolicy source, RateLimitPolicyDto destination);
}

[Mapper]
public partial class CreateOrEditRateLimitPolicyDtoToRateLimitPolicyMapper : MapperBase<CreateOrEditRateLimitPolicyDto, RateLimitPolicy>
{
    public override partial RateLimitPolicy Map(CreateOrEditRateLimitPolicyDto source);

    public override partial void Map(CreateOrEditRateLimitPolicyDto source, RateLimitPolicy destination);
}

[Mapper]
public partial class RateLimitPolicyToCreateOrEditRateLimitPolicyDtoMapper : MapperBase<RateLimitPolicy, CreateOrEditRateLimitPolicyDto>
{
    public override partial CreateOrEditRateLimitPolicyDto Map(RateLimitPolicy source);

    public override partial void Map(RateLimitPolicy source, CreateOrEditRateLimitPolicyDto destination);
}
