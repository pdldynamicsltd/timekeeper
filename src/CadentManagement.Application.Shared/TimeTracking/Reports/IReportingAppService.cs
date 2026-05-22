using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.Dto;
using CadentManagement.TimeTracking.Reports.Dto;
using CadentManagement.TimeTracking.TimeEntries.Dto;

namespace CadentManagement.TimeTracking.Reports;

public interface IReportingAppService : IApplicationService
{
    Task<ProjectBudgetReportDto> GetProjectBudgetReportAsync(EntityDto<int> input);

    Task<PagedResultDto<TimeEntryDto>> GetTimeEntriesReportAsync(GetTimeEntriesReportInput input);

    Task<FileDto> ExportToExcelAsync(GetTimeEntriesReportInput input);
}
