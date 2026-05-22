using Abp.Localization;
using CadentManagement.Install.Dto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CadentManagement.Web.Models.Install;

public class InstallSetupViewModel
{
    [Required]
    public string ConnectionString { get; set; }

    [Required]
    [MinLength(6)]
    public string AdminPassword { get; set; }

    [Required]
    [Compare("AdminPassword", ErrorMessage = "Passwords do not match.")]
    public string AdminPasswordRepeat { get; set; }

    public string WebSiteUrl { get; set; }

    [EmailAddress(ErrorMessage = "Email must be in a valid format.")]
    public string DefaultFromAddress { get; set; }

    [Required]
    public string DefaultFromDisplayName { get; set; }

    public string SmtpHost { get; set; }

    public int? SmtpPort { get; set; }

    public bool SmtpEnableSsl { get; set; }

    public bool SmtpUseAuthentication { get; set; }

    public string SmtpDomain { get; set; }

    public string SmtpUserName { get; set; }

    public string SmtpPassword { get; set; }

    public string LegalName { get; set; }

    public string BillAddress { get; set; }

    public string DefaultLanguage { get; set; }

    public List<ApplicationLanguage> Languages { get; set; }
    public AppSettingsJsonDto AppSettingsJson { get; set; }

}

