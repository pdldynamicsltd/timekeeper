using System;
using System.Web;
using Abp.Runtime.Security;
using CadentManagement.Authorization.Accounts.Dto;
using CadentManagement.Security;

namespace CadentManagement.Web.Models.Account;

public class ResetPasswordViewModel : ResetPasswordInput
{
    public int? TenantId { get; set; }

    public string ReturnUrl { get; set; }

    public string SingleSignIn { get; set; }

    public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

    protected override void ResolveParameters()
    {
        base.ResolveParameters();

        if (!string.IsNullOrEmpty(c))
        {
            var parameters = SimpleStringCipher.Instance.Decrypt(c);
            var query = HttpUtility.ParseQueryString(parameters);

            if (query["tenantId"] != null)
            {
                TenantId = Convert.ToInt32(query["tenantId"]);
            }
        }
    }
}

