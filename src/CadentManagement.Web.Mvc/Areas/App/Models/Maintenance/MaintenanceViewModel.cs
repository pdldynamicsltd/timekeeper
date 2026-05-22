using System.Collections.Generic;
using CadentManagement.Caching.Dto;

namespace CadentManagement.Web.Areas.App.Models.Maintenance;

public class MaintenanceViewModel
{
    public IReadOnlyList<CacheDto> Caches { get; set; }

    public bool CanClearAllCaches { get; set; }
}

