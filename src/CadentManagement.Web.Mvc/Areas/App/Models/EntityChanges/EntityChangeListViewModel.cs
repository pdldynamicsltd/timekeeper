using CadentManagement.EntityChanges.Dto;
using System.Collections.Generic;

namespace CadentManagement.Web.Areas.App.Models.EntityChanges;

public class EntityChangeListViewModel
{
    public List<EntityAndPropertyChangeListDto> EntityAndPropertyChanges { get; set; }
}

