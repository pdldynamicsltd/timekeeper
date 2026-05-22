using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;
using Abp.Auditing;

namespace CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

public class MergeExternalLoginInput
{
    [Required]
    [MaxLength(UserLogin.MaxLoginProviderLength)]
    public string AuthProvider { get; set; }

    [Required]
    [MaxLength(UserLogin.MaxProviderKeyLength)]
    public string ProviderKey { get; set; }

    [Required]
    [DisableAuditing]
    public string ProviderAccessCode { get; set; }

    [Required]
    [DisableAuditing]
    public string TargetUserPassword { get; set; }
}
