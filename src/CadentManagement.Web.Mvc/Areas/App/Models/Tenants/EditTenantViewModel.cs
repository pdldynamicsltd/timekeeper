using System.Collections.Generic;
using CadentManagement.Editions.Dto;
using CadentManagement.MultiTenancy.Dto;

namespace CadentManagement.Web.Areas.App.Models.Tenants;

public class EditTenantViewModel
{
    public TenantEditDto Tenant { get; set; }

    public IReadOnlyList<SubscribableEditionComboboxItemDto> EditionItems { get; set; }

    public EditTenantViewModel(TenantEditDto tenant, IReadOnlyList<SubscribableEditionComboboxItemDto> editionItems)
    {
        Tenant = tenant;
        EditionItems = editionItems;
    }
}

