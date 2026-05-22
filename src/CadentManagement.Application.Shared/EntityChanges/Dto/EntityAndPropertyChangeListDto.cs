using System.Collections.Generic;

namespace CadentManagement.EntityChanges.Dto;

public class EntityAndPropertyChangeListDto
{
    public EntityChangeListDto EntityChange { get; set; }
    public List<EntityPropertyChangeDto> EntityPropertyChanges { get; set; }
}

