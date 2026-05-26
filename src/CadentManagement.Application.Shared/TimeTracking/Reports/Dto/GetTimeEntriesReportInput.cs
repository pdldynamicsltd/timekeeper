using System;
using Abp.Application.Services.Dto;
using CadentManagement.Dto;

namespace CadentManagement.TimeTracking.Reports.Dto;

[Serializable]
public class GetTimeEntriesReportInput
{
    public int? ProjectId { get; set; }

    public int? TaskId { get; set; }

    public long? UserId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
