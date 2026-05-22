using System.Security.Claims;
using CadentManagement.Authorization.Users;

namespace CadentManagement.Authorization.Impersonation;

public class UserAndIdentity
{
    public User User { get; set; }

    public ClaimsIdentity Identity { get; set; }

    public UserAndIdentity(User user, ClaimsIdentity identity)
    {
        User = user;
        Identity = identity;
    }
}

