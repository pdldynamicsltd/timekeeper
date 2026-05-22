using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetZeroCore.Net;
using Abp.Auditing;
using Abp.Extensions;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Authorization.Users.Profile;
using CadentManagement.Authorization.Users.Profile.Dto;
using CadentManagement.Storage;
using System;
using System.Threading.Tasks;

namespace CadentManagement.Web.Controllers;

[AbpMvcAuthorize]
[DisableAuditing]
public class ProfileController : ProfileControllerBase
{
    private readonly IProfileAppService _profileAppService;
    public ProfileController(
        ITempFileCacheManager tempFileCacheManager,
        IProfileAppService profileAppService)
        : base(tempFileCacheManager, profileAppService)
    {
        _profileAppService = profileAppService;
    }

    public async Task<FileResult> GetProfilePicture()
    {
        var output = await _profileAppService.GetProfilePicture();

        if (output.ProfilePicture.IsNullOrEmpty())
        {
            return GetDefaultProfilePictureInternal();
        }

        return File(Convert.FromBase64String(output.ProfilePicture), MimeTypeNames.ImageJpeg);
    }

    public virtual async Task<FileResult> GetFriendProfilePicture(long userId, int? tenantId)
    {
        var output = await _profileAppService.GetFriendProfilePicture(new GetFriendProfilePictureInput()
        {
            TenantId = tenantId,
            UserId = userId
        });

        if (output.ProfilePicture.IsNullOrEmpty())
        {
            return GetDefaultProfilePictureInternal();
        }

        return File(Convert.FromBase64String(output.ProfilePicture), MimeTypeNames.ImageJpeg);
    }
}

