using System.Collections.Generic;

namespace CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

public class ExternalLoginProviderDto
{
    public string Name { get; set; }

    public string ClientId { get; set; }

    public bool IsLinked { get; set; }

    public bool CanUnlink { get; set; }

    public string EmailAddress { get; set; }

    public Dictionary<string, string> AdditionalParams { get; set; }
}
