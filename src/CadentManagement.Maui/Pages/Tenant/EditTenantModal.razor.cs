using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Components;
using CadentManagement.Common;
using CadentManagement.Editions.Dto;
using CadentManagement.Maui.Core.Components;
using CadentManagement.Maui.Core.Threading;
using CadentManagement.Maui.Models.Tenants;
using CadentManagement.MultiTenancy;
using CadentManagement.MultiTenancy.Dto;

namespace CadentManagement.Maui.Pages.Tenant;

public partial class EditTenantModal : CadentManagementMainLayoutPageComponentBase
{
    [Parameter] public int TenantId { get; set; }

    private readonly ITenantAppService _tenantAppService;

    private readonly ICommonLookupAppService _commonLookupAppService;

    private EditTenantModel EditTenantModel { get; set; } = new();

    public EditTenantModal()
    {
        _tenantAppService = Resolve<ITenantAppService>();
        _commonLookupAppService = Resolve<ICommonLookupAppService>();
    }

    protected override async Task OnInitializedAsync()
    {
        EditTenantModel = new EditTenantModel();

        await SetBusyAsync(async () =>
        {
            var tenant = await _tenantAppService.GetTenantForEdit(new EntityDto(TenantId));
            EditTenantModel = ObjectMapper.Map<EditTenantModel>(tenant);
            EditTenantModel.IsUnlimitedTimeSubscription = EditTenantModel.SubscriptionEndDateUtc == null;
            await PopulateEditionsCombobox();
        });

        await SetPageHeader(L("EditTenant"));
    }


    private async Task UpdateTenantAsync()
    {
        await SetBusyAsync(async () =>
        {
            await WebRequestExecuter.Execute(async () =>
            {
                EditTenantModel.NormalizeEditTenantInputModel();
                var input = ObjectMapper.Map<TenantEditDto>(EditTenantModel);

                await _tenantAppService.UpdateTenant(input);
            }, async () => { await UserDialogsService.AlertSuccess(L("SuccessfullySaved")); });
        });
    }

    private async Task PopulateEditionsCombobox()
    {
        var editions = await _commonLookupAppService.GetEditionsForCombobox();
        EditTenantModel.Editions = editions.Items.ToList();

        EditTenantModel.Editions.Insert(0, new SubscribableEditionComboboxItemDto(EditTenantModel.NotAssignedValue,
            $"- {L("NotAssigned")} -", null));

        EditTenantModel.SelectedEdition = EditTenantModel.EditionId?.ToString() ?? EditTenantModel.NotAssignedValue;
    }
}