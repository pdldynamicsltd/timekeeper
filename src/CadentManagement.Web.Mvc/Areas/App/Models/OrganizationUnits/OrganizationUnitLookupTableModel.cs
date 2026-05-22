using System.Collections.Generic;
using CadentManagement.Organizations.Dto;
using CadentManagement.Web.Areas.App.Models.Common;

namespace CadentManagement.Web.Areas.App.Models.OrganizationUnits;

public class OrganizationUnitLookupTableModel : IOrganizationUnitsEditViewModel
{
    public List<OrganizationUnitDto> AllOrganizationUnits { get; set; }

    public List<string> MemberedOrganizationUnits { get; set; }

    public OrganizationUnitLookupTableModel()
    {
        AllOrganizationUnits = new List<OrganizationUnitDto>();
        MemberedOrganizationUnits = new List<string>();
    }
}

