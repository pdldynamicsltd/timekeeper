using System.Collections.Generic;
using CadentManagement.HealthChecks.Dto;

namespace CadentManagement.Web.Areas.App.Models.HealthCheckUI;

public class HealthCheckUIViewModel
{
    public IReadOnlyList<HealthCheckItemDto> HealthChecks { get; set; }
}
