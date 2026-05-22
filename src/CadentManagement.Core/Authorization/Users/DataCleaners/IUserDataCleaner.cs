using Abp;
using System.Threading.Tasks;

namespace CadentManagement.Authorization.Users.DataCleaners;

public interface IUserDataCleaner
{
    Task CleanUserData(UserIdentifier userIdentifier);
}

