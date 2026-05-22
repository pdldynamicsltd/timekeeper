using Abp.EntityHistory;
using Abp.Mapperly;
using CadentManagement.EntityChanges;
using CadentManagement.EntityChanges.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class EntityChangeToEntityChangeListDtoMapper : MapperBase<EntityChange, EntityChangeListDto>
{
    public override partial EntityChangeListDto Map(EntityChange source);

    public override partial void Map(EntityChange source, EntityChangeListDto destination);
}

public class EntityChangeToEntityAndPropertyChangeListDtoMapper : MapperBase<EntityChange, EntityAndPropertyChangeListDto>
{
    public override EntityAndPropertyChangeListDto Map(EntityChange source)
    {
        return new EntityAndPropertyChangeListDto();
    }

    public override void Map(EntityChange source, EntityAndPropertyChangeListDto destination)
    {
    }
}

[Mapper]
public partial class EntityPropertyChangeToEntityPropertyChangeDtoMapper : MapperBase<EntityPropertyChange, EntityPropertyChangeDto>
{
    public override partial EntityPropertyChangeDto Map(EntityPropertyChange source);

    public override partial void Map(EntityPropertyChange source, EntityPropertyChangeDto destination);
}

[Mapper]
public partial class EntityChangePropertyAndUserToEntityChangeListDtoMapper : MapperBase<EntityChangePropertyAndUser, EntityChangeListDto>
{
    public override partial EntityChangeListDto Map(EntityChangePropertyAndUser source);

    public override partial void Map(EntityChangePropertyAndUser source, EntityChangeListDto destination);
}
