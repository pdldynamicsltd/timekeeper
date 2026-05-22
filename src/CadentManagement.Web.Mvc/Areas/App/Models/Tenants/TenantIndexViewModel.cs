using System.Collections.Generic;
using CadentManagement.Editions.Dto;

namespace CadentManagement.Web.Areas.App.Models.Tenants;

public class TenantIndexViewModel
{
    public List<SubscribableEditionComboboxItemDto> EditionItems { get; set; }
}

