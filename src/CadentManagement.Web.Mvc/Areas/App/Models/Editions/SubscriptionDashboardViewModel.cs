using CadentManagement.MultiTenancy.Dto;
using CadentManagement.Sessions.Dto;

namespace CadentManagement.Web.Areas.App.Models.Editions;

public class SubscriptionDashboardViewModel
{
    public GetCurrentLoginInformationsOutput LoginInformations { get; set; }

    public EditionsSelectOutput Editions { get; set; }
}

