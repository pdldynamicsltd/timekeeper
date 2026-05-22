using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.Authorization.Session.Dto;

namespace CadentManagement.Authorization.Session;

public interface IUserSessionAppService : IApplicationService
{
    Task<ListResultDto<UserSessionDto>> GetSessions(GetUserSessionsInput input);

    Task RevokeSession(EntityDto<long> input);

    Task RevokeAllOtherSessions();
}
