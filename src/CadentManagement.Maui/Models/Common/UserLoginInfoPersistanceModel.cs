using Abp.Application.Services.Dto;

namespace CadentManagement.Maui.Models.Common;

public class UserLoginInfoPersistanceModel : EntityDto<long>
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public string UserName { get; set; }

    public string EmailAddress { get; set; }

    public string ProfilePictureId { get; set; }
}