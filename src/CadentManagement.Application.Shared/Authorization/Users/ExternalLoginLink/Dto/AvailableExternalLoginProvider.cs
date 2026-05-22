using System.Collections.Generic;

namespace CadentManagement.Authorization.Users.ExternalLoginLink.Dto;

public class AvailableExternalLoginProvider
{
    public string Name { get; set; }

    public string ClientId { get; set; }

    public Dictionary<string, string> AdditionalParams { get; set; }
}
