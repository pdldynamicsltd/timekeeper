using Abp.Zero.Ldap.Authentication;
using Abp.Zero.Ldap.Configuration;
using CadentManagement.Authorization.Users;
using CadentManagement.MultiTenancy;

namespace CadentManagement.Authorization.Ldap;

public class AppLdapAuthenticationSource : LdapAuthenticationSource<Tenant, User>
{
    public AppLdapAuthenticationSource(ILdapSettings settings, IAbpZeroLdapModuleConfig ldapModuleConfig)
        : base(settings, ldapModuleConfig)
    {
    }
}

