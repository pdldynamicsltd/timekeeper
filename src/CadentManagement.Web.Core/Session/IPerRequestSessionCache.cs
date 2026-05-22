using System.Threading.Tasks;
using CadentManagement.Sessions.Dto;

namespace CadentManagement.Web.Session;

public interface IPerRequestSessionCache
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformationsAsync();
}

