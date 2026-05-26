using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Tasks.Dto;

[Serializable]
public class GetTaskForEditOutput
{
    public CreateOrEditTaskDto Task { get; set; }

    public List<ComboboxItemDto> ProjectOptions { get; set; } = new();

    public List<ComboboxItemDto> StatusOptions { get; set; } = new();

    public List<ComboboxItemDto> ParentTaskOptions { get; set; } = new();

    public List<ComboboxItemDto> UserOptions { get; set; } = new();
}
