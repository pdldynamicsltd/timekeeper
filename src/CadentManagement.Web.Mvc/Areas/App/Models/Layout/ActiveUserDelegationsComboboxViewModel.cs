using System.Collections.Generic;
using CadentManagement.Authorization.Delegation;
using CadentManagement.Authorization.Users.Delegation.Dto;

namespace CadentManagement.Web.Areas.App.Models.Layout;

public class ActiveUserDelegationsComboboxViewModel
{
    public IUserDelegationConfiguration UserDelegationConfiguration { get; set; }

    public List<UserDelegationDto> UserDelegations { get; set; }

    public string CssClass { get; set; }
}

