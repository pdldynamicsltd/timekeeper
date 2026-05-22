using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Abp.Organizations;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization;
using CadentManagement.Notifications;
using CadentManagement.Organizations;
using CadentManagement.Organizations.Dto;
using CadentManagement.Web.Areas.App.Models.Notifications;
using CadentManagement.Web.Areas.App.Models.OrganizationUnits;
using CadentManagement.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadentManagement.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize]
public class NotificationsController : CadentManagementControllerBase
{
    private readonly INotificationAppService _notificationAppService;
    private readonly IOrganizationUnitAppService _organizationUnitAppService;

    public NotificationsController(
        INotificationAppService notificationAppService,
        IOrganizationUnitAppService organizationUnitAppService)
    {
        _notificationAppService = notificationAppService;
        _organizationUnitAppService = organizationUnitAppService;
    }

    public ActionResult Index()
    {
        return View();
    }

    public async Task<PartialViewResult> SettingsModal()
    {
        var notificationSettings = await _notificationAppService.GetNotificationSettings();
        return PartialView("_SettingsModal", notificationSettings);
    }

    [AbpMvcAuthorize(AppPermissions.Pages_Administration_MassNotification_Create)]
    public PartialViewResult CreateMassNotificationModal()
    {
        var viewModel = new CreateMassNotificationViewModel
        {
            TargetNotifiers = _notificationAppService.GetAllNotifiers()
        };

        return PartialView("_CreateMassNotificationModal", viewModel);
    }

    [AbpMvcAuthorize(AppPermissions.Pages_Administration_MassNotification)]
    public PartialViewResult UserLookupTableModal()
    {
        return PartialView("_UserLookupTableModal");
    }

    [AbpMvcAuthorize(AppPermissions.Pages_Administration_MassNotification)]
    public async Task<PartialViewResult> OrganizationUnitLookupTableModal()
    {
        var organizationUnits = await _organizationUnitAppService.GetAll();
        var model = new OrganizationUnitLookupTableModel
        {
            AllOrganizationUnits = organizationUnits
        };

        return PartialView("_OrganizationUnitLookupTableModal", model);
    }


    [AbpMvcAuthorize(AppPermissions.Pages_Administration_MassNotification)]
    public ActionResult MassNotifications()
    {
        return View();
    }

    public async Task<PartialViewResult> ViewModal(Guid id)
    {
        var dto = await _notificationAppService.GetNotificationDetail(
            new EntityDto<Guid>(id)
        );

        var model = new NotificationDetailViewModel
        {
            Title = dto.Title,
            Message = dto.Message,
            Severity = dto.Severity,
            CreationTime = dto.CreationTime
        };

        return PartialView("_ViewModal", model);
    }
}

