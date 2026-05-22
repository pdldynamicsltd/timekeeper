using System;
using System.Web;
using Abp.Auditing;
using Abp.Runtime.Security;
using Abp.Runtime.Validation;

namespace CadentManagement.Authorization.Accounts.Dto;

public class ResetPasswordInput : IShouldNormalize
{
    public long UserId { get; set; }

    public string ResetCode { get; set; }

    [DisableAuditing]
    public string Password { get; set; }

    /// <summary>
    /// Encrypted values for {TenantId}, {UserId} and {ResetCode}.
    /// </summary>
    public string c { get; set; }

    public void Normalize()
    {
        ResolveParameters();
    }

    protected virtual void ResolveParameters()
    {
        if (!string.IsNullOrEmpty(c))
        {
            try
            {
                var parameters = SimpleStringCipher.Instance.Decrypt(c);
                var query = HttpUtility.ParseQueryString(parameters);

                if (query["userId"] != null)
                {
                    UserId = Convert.ToInt32(query["userId"]);
                }

                if (query["resetCode"] != null)
                {
                    ResetCode = query["resetCode"];
                }
            }
            catch
            {
                throw new AbpValidationException("Invalid reset password link!");
            }
        }
    }
}
