using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Projects.Dto;

[Serializable]
public class GetProjectForEditOutput
{
    public CreateOrEditProjectDto Project { get; set; }

    public List<ComboboxItemDto> BudgetTypeOptions { get; set; } = new();

    public List<ComboboxItemDto> StatusOptions { get; set; } = new();
}
