using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Microsoft.EntityFrameworkCore;
using CadentManagement.TimeTracking.Dto;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.TimeEntries;
using Shouldly;
using Xunit;

namespace CadentManagement.Tests.TimeTracking;

public class TimeTrackingImportAppService_Tests : AppTestBase
{
    private readonly IProjectAppService _projectAppService;
    private readonly ITimeEntryAppService _timeEntryAppService;

    public TimeTrackingImportAppService_Tests()
    {
        _projectAppService = Resolve<IProjectAppService>();
        _timeEntryAppService = Resolve<ITimeEntryAppService>();
    }

    [Fact]
    public async Task Should_Import_Projects_From_Csv()
    {
        var csv = "\"Project\",\"Client\",\"Status\",\"Visibility\",\"Billability\",\"Task\",\"Tracked (h)\",\"Estimated (h)\",\"Remaining (h)\",\"Overage (h)\",\"Progress(%)\",\"Billable (h)\",\"Non-billable (h)\",\"Billable Rate (USD)\",\"Amount (USD)\",\"Project members\",\"Project manager\",\"Note\"\n" +
                  "\"Import Test Project\",\"\",\"Active\",\"Public\",\"Yes\",\"\",\"0\",\"12.5\",\"\",\"\",\"\",\"0\",\"0\",\"\",\"0\",\"\",\"\",\"Imported note\"";

        var result = await _projectAppService.ImportProjectsFromCsvAsync(new ImportProjectsCsvInput
        {
            CsvContent = csv
        });

        result.CreatedProjects.ShouldBe(1);
        result.SkippedRows.ShouldBe(0);

        await UsingDbContextAsync(async context =>
        {
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Name == "Import Test Project");
            project.ShouldNotBeNull();
            project.BudgetHours.ShouldBe(12.5m);

            var tracking = await context.ProjectBudgetTrackings.FirstOrDefaultAsync(t => t.ProjectId == project.Id);
            tracking.ShouldNotBeNull();
            tracking.TotalBudgetHours.ShouldBe(12.5m);
        });
    }

    [Fact]
    public async Task Should_Import_Time_And_Create_Missing_Project_And_Task()
    {
        var csv = "\"Project\",\"Client\",\"Description\",\"Task\",\"Kiosk\",\"User\",\"Group\",\"Email\",\"Tags\",\"Billable\",\"Start Date\",\"Start Time\",\"End Date\",\"End Time\",\"Duration (h)\",\"Duration (decimal)\",\"Billable Rate (USD)\",\"Billable Amount (USD)\",\"Date of creation\"\n" +
                  "\"Import Time Project\",\"\",\"Imported work\",\"Imported Task\",\"\",\"Paul Lewis\",\"\",\"test@test.com\",\"\",\"Yes\",\"20/05/2026\",\"08:00:00\",\"20/05/2026\",\"10:30:00\",\"02:30:00\",\"2.5\",\"0\",\"0\",\"21/05/2026\"";

        var result = await _timeEntryAppService.ImportTimeEntriesFromCsvAsync(new ImportTimeEntriesCsvInput
        {
            CsvContent = csv
        });

        result.CreatedProjects.ShouldBe(1);
        result.CreatedTasks.ShouldBe(1);
        result.CreatedTimeEntries.ShouldBe(1);
        result.SkippedRows.ShouldBe(0);

        var currentUserId = AbpSession.UserId.ShouldNotBeNull();

        await UsingDbContextAsync(async context =>
        {
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Name == "Import Time Project");
            project.ShouldNotBeNull();

            var task = await context.ProjectTasks.FirstOrDefaultAsync(t => t.ProjectId == project.Id && t.Name == "Imported Task");
            task.ShouldNotBeNull();

            var timeEntry = await context.TimeEntries.FirstOrDefaultAsync(e =>
                e.ProjectId == project.Id && e.TaskId == task.Id && e.Description == "Imported work");

            timeEntry.ShouldNotBeNull();
            timeEntry.UserId.ShouldBe(currentUserId);
            timeEntry.StartTime.ShouldBe(new DateTime(2026, 5, 20, 8, 0, 0));
            timeEntry.EndTime.ShouldBe(new DateTime(2026, 5, 20, 10, 30, 0));
        });

        var importedProjectId = await UsingDbContextAsync(async context =>
            await context.Projects.Where(p => p.Name == "Import Time Project").Select(p => p.Id).FirstAsync());
        var summary = await _projectAppService.GetProjectBudgetSummaryAsync(new EntityDto<int>(importedProjectId));
        summary.UsedHours.ShouldBe(2.5m);
    }
}
