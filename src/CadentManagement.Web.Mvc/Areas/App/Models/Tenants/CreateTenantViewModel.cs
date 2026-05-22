using System.Collections.Generic;
using CadentManagement.Editions.Dto;
using CadentManagement.Security;

namespace CadentManagement.Web.Areas.App.Models.Tenants;

public class CreateTenantViewModel
{
    public IReadOnlyList<SubscribableEditionComboboxItemDto> EditionItems { get; set; }

    public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

    public CreateTenantViewModel(IReadOnlyList<SubscribableEditionComboboxItemDto> editionItems)
    {
        EditionItems = editionItems;
    }
}

