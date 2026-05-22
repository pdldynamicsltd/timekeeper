using System.ComponentModel.DataAnnotations;
using Abp.Auditing;

namespace CadentManagement.Web.Models.Account;

public class ServerSideMergeExternalLoginInput
{
    [Required]
    public string MergeToken { get; set; }

    [Required]
    [DisableAuditing]
    public string TargetUserPassword { get; set; }
}
