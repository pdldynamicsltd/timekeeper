using Abp.Application.Services.Dto;

namespace CadentManagement.Notifications.Dto;

public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}

