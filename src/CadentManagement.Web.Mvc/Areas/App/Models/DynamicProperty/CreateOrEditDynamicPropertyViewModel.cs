using System.Collections.Generic;
using CadentManagement.DynamicEntityProperties.Dto;

namespace CadentManagement.Web.Areas.App.Models.DynamicProperty;

public class CreateOrEditDynamicPropertyViewModel
{
    public DynamicPropertyDto DynamicPropertyDto { get; set; }

    public List<string> AllowedInputTypes { get; set; }
}

