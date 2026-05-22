using System.ComponentModel.DataAnnotations;

namespace CadentManagement.Web.Models.Account;

public class SendPasswordResetLinkViewModel
{
    [Required]
    public string EmailAddress { get; set; }
}

