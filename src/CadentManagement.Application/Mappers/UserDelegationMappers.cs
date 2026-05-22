using Abp.Mapperly;
using CadentManagement.Authorization.Delegation;
using CadentManagement.Authorization.Users.Delegation.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class CreateUserDelegationDtoToUserDelegationMapper : MapperBase<CreateUserDelegationDto, UserDelegation>
{
    public override partial UserDelegation Map(CreateUserDelegationDto source);

    public override partial void Map(CreateUserDelegationDto source, UserDelegation destination);
}
