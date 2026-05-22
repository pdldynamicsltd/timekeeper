using System.Collections.Generic;

namespace CadentManagement.Web.Models.TokenAuth;

public class ExternalLoginProviderInfoModel
{
    public string Name { get; set; }

    public string ClientId { get; set; }

    public Dictionary<string, string> AdditionalParams { get; set; }

}

