using Microsoft.AspNetCore.Antiforgery;

namespace CadentManagement.Web.Controllers;

public class AntiForgeryController : CadentManagementControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AntiForgeryController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public void GetToken()
    {
        _antiforgery.SetCookieTokenAndHeader(HttpContext);
    }
}

