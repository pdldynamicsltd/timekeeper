using System.Collections.Generic;
using CadentManagement.Organizations.Dto;

namespace CadentManagement.Web.Areas.App.Models.Common;

public interface IOrganizationUnitsEditViewModel
{
    List<OrganizationUnitDto> AllOrganizationUnits { get; set; }

    List<string> MemberedOrganizationUnits { get; set; }
}

