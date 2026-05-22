using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace CadentManagement.Authorization.Users;

[Table("AppUserExternalLoginInfos")]
public class UserExternalLoginInfo : Entity<long>, IMayHaveTenant, IHasCreationTime
{
    public const int MaxLoginProviderLength = UserLogin.MaxLoginProviderLength;

    public const int MaxEmailAddressLength = AbpUserBase.MaxEmailAddressLength;

    public int? TenantId { get; set; }

    public long UserId { get; set; }

    public string LoginProvider { get; set; }

    [DisableAuditing]
    public string EmailAddress { get; set; }

    public DateTime CreationTime { get; set; }
}
