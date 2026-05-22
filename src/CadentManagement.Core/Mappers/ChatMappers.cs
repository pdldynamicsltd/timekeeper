using Abp.Mapperly;
using CadentManagement.Friendships;
using CadentManagement.Friendships.Cache;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class FriendshipToFriendCacheItemMapper : MapperBase<Friendship, FriendCacheItem>
{
    public override partial FriendCacheItem Map(Friendship source);

    public override partial void Map(Friendship source, FriendCacheItem destination);
}
