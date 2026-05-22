using System.ComponentModel.DataAnnotations;

namespace CadentManagement.Organizations.Dto;

public class MoveOrganizationUnitInput
{
    [Range(1, long.MaxValue)]
    public long Id { get; set; }

    public long? NewParentId { get; set; }
}

