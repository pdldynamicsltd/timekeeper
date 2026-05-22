using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;

namespace CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

public class UnlinkExternalLoginInput
{
    [Required]
    [MaxLength(UserLogin.MaxLoginProviderLength)]
    public string AuthProvider { get; set; }
}
