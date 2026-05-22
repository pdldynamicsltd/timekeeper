using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CadentManagement.Web.Authentication.JwtBearer;

public class AsyncJwtBearerOptions : JwtBearerOptions
{
    public readonly List<IAsyncSecurityTokenValidator> AsyncSecurityTokenValidators;

    private readonly CadentManagementAsyncJwtSecurityTokenHandler _defaultAsyncHandler = new CadentManagementAsyncJwtSecurityTokenHandler();

    public AsyncJwtBearerOptions()
    {
        AsyncSecurityTokenValidators = new List<IAsyncSecurityTokenValidator>() { _defaultAsyncHandler };
    }
}


