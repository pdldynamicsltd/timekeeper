using System.Threading.Tasks;
using Abp.Application.Services;
using CadentManagement.Sessions.Dto;

namespace CadentManagement.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();

    Task<UpdateUserSignInTokenOutput> UpdateUserSignInToken();
}

