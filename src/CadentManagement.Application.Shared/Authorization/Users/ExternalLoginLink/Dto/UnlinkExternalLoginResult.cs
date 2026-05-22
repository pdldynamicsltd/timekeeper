namespace CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

public class UnlinkExternalLoginResult
{
    public bool Success { get; set; }

    public bool RequiresPasswordSetup { get; set; }
}
