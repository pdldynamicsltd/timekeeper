using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using CadentManagement.Authorization.Users.Profile.Dto;

namespace CadentManagement.Web.Areas.App.Models.Profile;

public class MySettingsViewModel : CurrentUserProfileEditDto
{
    public List<ComboboxItemDto> TimezoneItems { get; set; }

    public bool SmsVerificationEnabled { get; set; }

    public bool CanChangeUserName => UserName != AbpUserBase.AdminUserName;

    public string Code { get; set; }
}

