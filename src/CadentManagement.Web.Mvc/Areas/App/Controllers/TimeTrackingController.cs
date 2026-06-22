using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.Tasks;
using CadentManagement.TimeTracking.TimeEntries;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_TimeTracking)]
public class TimeTrackingController : CadentManagementControllerBase
{
    private readonly IProjectAppService _projectAppService;
    private readonly IProjectTaskAppService _taskAppService;
    private readonly ITimeEntryAppService _timeEntryAppService;

    public TimeTrackingController(
        IProjectAppService projectAppService,
        IProjectTaskAppService taskAppService,
        ITimeEntryAppService timeEntryAppService)
    {
        _projectAppService = projectAppService;
        _taskAppService = taskAppService;
        _timeEntryAppService = timeEntryAppService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> ProjectDetail(int id)
    {
        var summary = await _projectAppService.GetProjectBudgetSummaryAsync(new EntityDto<int>(id));
        return View(summary);
    }

    public IActionResult Reports()
    {
        return View();
    }

    public IActionResult MyWeek()
    {
        return View();
    }

    [AbpMvcAuthorize(AppPermissions.Pages_TimeTracking_Projects_Create, AppPermissions.Pages_TimeTracking_Projects_Edit)]
    public async Task<IActionResult> CreateOrEditProjectModal(int? id)
    {
        var output = await _projectAppService.GetForEditAsync(new NullableIdDto<int>(id));
        return PartialView("_CreateOrEditProjectModal", output);
    }

    [AbpMvcAuthorize(AppPermissions.Pages_TimeTracking_Tasks_Create, AppPermissions.Pages_TimeTracking_Tasks_Edit)]
    public async Task<IActionResult> CreateOrEditTaskModal(int? id, int? projectId)
    {
        var output = await _taskAppService.GetForEditAsync(new NullableIdDto<int>(id));
        if (!id.HasValue && projectId.HasValue)
        {
            output.Task.ProjectId = projectId.Value;
        }
        return PartialView("_CreateOrEditTaskModal", output);
    }

    [AbpMvcAuthorize(AppPermissions.Pages_TimeTracking_TimeEntries_Create, AppPermissions.Pages_TimeTracking_TimeEntries_Edit)]
    public async Task<IActionResult> CreateOrEditTimeEntryModal(int? id, int? projectId, int? taskId, string startTime = null, string endTime = null, string description = null)
    {
        var output = await _timeEntryAppService.GetForEditAsync(new NullableIdDto<int>(id));

        if (!id.HasValue)
        {
            if (projectId.HasValue)
            {
                output.TimeEntry.ProjectId = projectId.Value;

                var projectTasks = await _taskAppService.GetTasksForProjectAsync(projectId.Value);
                output.TaskOptions = projectTasks
                    .Select(t => new ComboboxItemDto(t.Id.ToString(), t.Name))
                    .ToList();
            }

            if (taskId.HasValue)
            {
                output.TimeEntry.TaskId = taskId.Value;
            }

            if (DateTime.TryParse(startTime, out var parsedStart))
            {
                output.TimeEntry.StartTime = parsedStart;
            }
            else
            {
                // Default new entries to 08:00 today
                var today = DateTime.Today;
                output.TimeEntry.StartTime = today.AddHours(8);
            }

            if (DateTime.TryParse(endTime, out var parsedEnd))
            {
                output.TimeEntry.EndTime = parsedEnd;
            }
            else
            {
                // Default new entries to 16:00 today
                var today = DateTime.Today;
                output.TimeEntry.EndTime = today.AddHours(16);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                output.TimeEntry.Description = description;
            }
        }

        return PartialView("_CreateOrEditTimeEntryModal", output);
    }
}