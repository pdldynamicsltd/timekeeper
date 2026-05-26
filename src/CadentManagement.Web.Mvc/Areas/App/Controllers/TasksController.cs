using System.Threading.Tasks;
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

    [AbpMvcAuthorize(AppPermissions.Pages_Tasks_Create, AppPermissions.Pages_Tasks_Edit)]
    public async Task<IActionResult> CreateOrEditUserTaskModal(int? id)
    {
        var output = await _userTaskAppService.GetForEditAsync(new Abp.Application.Services.Dto.NullableIdDto<int>(id));
        return PartialView("_CreateOrEditUserTaskModal", output);
    }
}
