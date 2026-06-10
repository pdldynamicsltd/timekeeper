using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.UserTasks;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Tasks)]
public class TasksController : CadentManagementControllerBase
{
    private readonly IUserTaskAppService _userTaskAppService;

    public TasksController(IUserTaskAppService userTaskAppService)
    {
        _userTaskAppService = userTaskAppService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Planner()
    {
        return View();
    }

    [AbpMvcAuthorize(AppPermissions.Pages_Tasks_Create, AppPermissions.Pages_Tasks_Edit)]
    public async Task<IActionResult> CreateOrEditUserTaskModal(int? id, int? projectId, int? status, DateTime? dueDate)
    {
        var output = await _userTaskAppService.GetForEditAsync(new NullableIdDto<int>(id));

        if (!id.HasValue && projectId.HasValue)
        {
            output.Task.ProjectId = projectId.Value;
        }

        if (!id.HasValue && status.HasValue)
        {
            output.Task.Status = (KanbanTaskStatus)status.Value;
        }

        if (!id.HasValue && dueDate.HasValue)
        {
            output.Task.DueDate = dueDate.Value.Date;
        }

        return PartialView("_CreateOrEditUserTaskModal", output);
    }
}