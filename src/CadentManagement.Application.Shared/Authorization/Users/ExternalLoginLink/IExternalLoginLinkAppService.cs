using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

namespace CadentManagement.Authorization.Users.ExternalLoginLink;

public interface IExternalLoginLinkAppService : IApplicationService
{
    Task<List<ExternalLoginProviderDto>> GetExternalLogins();

    Task<LinkExternalLoginResult> LinkExternalLogin(LinkExternalLoginInput input);

    Task<UnlinkExternalLoginResult> UnlinkExternalLogin(UnlinkExternalLoginInput input);

    Task MergeAndLinkExternalLogin(MergeExternalLoginInput input);
}
