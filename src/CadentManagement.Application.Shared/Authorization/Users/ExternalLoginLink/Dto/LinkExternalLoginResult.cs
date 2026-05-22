namespace CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

public class LinkExternalLoginResult
{
    public bool Success { get; set; }

    public bool ProviderAlreadyLinkedToAnotherUser { get; set; }

    public bool CanMerge { get; set; }

    public string ExistingUserEmail { get; set; }
}
