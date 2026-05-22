namespace CadentManagement.Authorization.Users.ExternalLoginLink;

public class ExternalLoginLinkResult
{
    public bool Success { get; set; }

    public bool ProviderAlreadyLinkedToAnotherUser { get; set; }

    public bool CanMerge { get; set; }

    public string ExistingUserEmail { get; set; }
}
