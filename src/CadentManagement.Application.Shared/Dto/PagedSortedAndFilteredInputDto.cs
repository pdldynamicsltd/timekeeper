using Abp.Application.Services.Dto;

namespace CadentManagement.Dto;

public class PagedSortedAndFilteredInputDto : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}

