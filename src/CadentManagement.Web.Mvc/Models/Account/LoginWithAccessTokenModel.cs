using System.ComponentModel.DataAnnotations;
using Abp.Auditing;

namespace CadentManagement.Web.Models.Account;

public class LoginWithAccessTokenModel
{
    public string AccessToken { get; set; }
}

