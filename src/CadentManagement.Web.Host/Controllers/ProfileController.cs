using Abp.AspNetCore.Mvc.Authorization;
using CadentManagement.Authorization.Users.Profile;
using CadentManagement.Storage;

namespace CadentManagement.Web.Controllers;

[AbpMvcAuthorize]
public class ProfileController : ProfileControllerBase
{
    public ProfileController(
        ITempFileCacheManager tempFileCacheManager,
        IProfileAppService profileAppService) :
        base(tempFileCacheManager, profileAppService)
    {
    }
}

