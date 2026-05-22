using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.EntityChanges;
using CadentManagement.EntityChanges.Dto;
using CadentManagement.Web.Areas.App.Models.EntityChanges;
using CadentManagement.Web.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_EntityChanges_FullHistory)]
public class EntityChangesController : CadentManagementControllerBase
{
    private readonly IEntityChangeAppService _entityChangeAppService;

    public EntityChangesController(IEntityChangeAppService entityChangeAppService)
    {
        _entityChangeAppService = entityChangeAppService;
    }

    [HttpGet]
    [Route("/App/EntityChanges/{entityId}/{entityTypeFullName}")]
    public async Task<IActionResult> Index(string entityId, string entityTypeFullName)
    {
        var entityChanges = await _entityChangeAppService.GetEntityChangesByEntity(new GetEntityChangesByEntityInput
        {
            EntityId = entityId,
            EntityTypeFullName = entityTypeFullName,
        });

        ViewBag.ChangesCount = entityChanges.Items.Count;
        ViewBag.EntityTypeShortName = entityTypeFullName.Substring(entityTypeFullName.LastIndexOf('.') + 1);
        ViewBag.EntityId = entityId;

        var viewModel = new EntityChangeListViewModel
        {
            EntityAndPropertyChanges = entityChanges.Items.ToList()
        };

        return View(viewModel);
    }
}

