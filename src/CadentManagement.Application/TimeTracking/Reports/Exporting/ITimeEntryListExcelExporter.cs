using System.Collections.Generic;
using System.Threading.Tasks;
using CadentManagement.Dto;
using CadentManagement.TimeTracking.TimeEntries.Dto;

namespace CadentManagement.TimeTracking.Reports.Exporting;

public interface ITimeEntryListExcelExporter
{
    Task<FileDto> ExportToFileAsync(List<TimeEntryDto> timeEntries);
}
