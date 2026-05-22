using System;
using Abp.Application.Services.Dto;
using CadentManagement.TimeTracking;

namespace CadentManagement.TimeTracking.Projects.Dto;

[Serializable]
public class GetProjectsInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }

    public ProjectStatus? StatusFilter { get; set; }
}
