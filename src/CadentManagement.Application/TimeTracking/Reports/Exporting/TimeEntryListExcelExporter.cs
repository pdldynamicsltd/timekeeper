using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CadentManagement.DataExporting.Excel.MiniExcel;
using CadentManagement.Dto;
using CadentManagement.Storage;
using CadentManagement.TimeTracking.TimeEntries.Dto;

namespace CadentManagement.TimeTracking.Reports.Exporting;

public class TimeEntryListExcelExporter : MiniExcelExcelExporterBase, ITimeEntryListExcelExporter
{
    public TimeEntryListExcelExporter(ITempFileCacheManager tempFileCacheManager)
        : base(tempFileCacheManager)
    {
    }

    public async Task<FileDto> ExportToFileAsync(List<TimeEntryDto> timeEntries)
    {
        var items = new List<Dictionary<string, object>>();

        foreach (var entry in timeEntries)
        {
            var durationHours = (int)entry.DurationHours;
            var durationMinutes = (int)((entry.DurationHours - durationHours) * 60);

            items.Add(new Dictionary<string, object>
            {
                { "Date", entry.StartTime.ToString("yyyy-MM-dd") },
                { "Start Time", entry.StartTime.ToString("HH:mm") },
                { "End Time", entry.EndTime.ToString("HH:mm") },
                { "Duration (HH:mm)", $"{durationHours:D2}:{durationMinutes:D2}" },
                { "Duration (Hours)", Math.Round(entry.DurationHours, 2) },
                { "Project", entry.ProjectName ?? "" },
                { "Task", entry.TaskName ?? "" },
                { "User", entry.UserFullName ?? entry.UserName ?? "" },
                { "Description", entry.Description ?? "" }
            });
        }

        return await CreateExcelPackageAsync("TimeEntries.xlsx", items);
    }
}
