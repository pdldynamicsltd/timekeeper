using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace CadentManagement.TimeTracking.TimeEntries.Dto;

[Serializable]
public class GetTimeEntryForEditOutput
{
    public CreateOrEditTimeEntryDto TimeEntry { get; set; }

    public List<ComboboxItemDto> ProjectOptions { get; set; } = new();

    public List<ComboboxItemDto> TaskOptions { get; set; } = new();
}
