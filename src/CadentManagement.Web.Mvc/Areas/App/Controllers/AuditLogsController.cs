using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Auditing;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Auditing;
using CadentManagement.Authorization;
using CadentManagement.EntityChanges.Dto;
using CadentManagement.Web.Areas.App.Models.AuditLogs;
using CadentManagement.Web.Controllers;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[DisableAuditing]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_AuditLogs)]
public class AuditLogsController : CadentManagementControllerBase
{
    private readonly IAuditLogAppService _auditLogAppService;

    public AuditLogsController(IAuditLogAppService auditLogAppService)
    {
        _auditLogAppService = auditLogAppService;
    }

    public ActionResult Index()
    {
        return View();
    }

    public async Task<PartialViewResult> EntityChangeDetailModal(EntityChangeListDto entityChangeListDto)
    {
        var output = await _auditLogAppService.GetEntityPropertyChanges(entityChangeListDto.Id);

        var viewModel = new EntityChangeDetailModalViewModel(output, entityChangeListDto);

        return PartialView("_EntityChangeDetailModal", viewModel);
    }
}

