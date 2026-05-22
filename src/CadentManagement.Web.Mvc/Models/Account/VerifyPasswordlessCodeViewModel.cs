using Abp.Localization;
using System.ComponentModel.DataAnnotations;

namespace CadentManagement.Web.Models.Account;

public class VerifyPasswordlessCodeViewModel
{
    [Required]
    [AbpDisplayName(CadentManagementConsts.LocalizationSourceName, "Code")]
    public string Code { get; set; }

    public string ProviderValue { get; set; }

    public string ProviderType { get; set; }

}

