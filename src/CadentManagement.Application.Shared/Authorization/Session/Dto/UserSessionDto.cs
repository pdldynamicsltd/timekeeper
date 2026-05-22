using System;
using Abp.Application.Services.Dto;

namespace CadentManagement.Authorization.Session.Dto;

public class UserSessionDto : EntityDto<long>
{
    public string IpAddress { get; set; }

    public string DeviceInfo { get; set; }

    public DateTime SignInTime { get; set; }

    public DateTime LastActivityTime { get; set; }

    public bool IsCurrent { get; set; }
}
