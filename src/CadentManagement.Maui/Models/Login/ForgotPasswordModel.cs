using System.ComponentModel.DataAnnotations;

namespace CadentManagement.Maui.Models.Login;

public class ForgotPasswordModel
{
    [EmailAddress]
    [Required]
    public string EmailAddress { get; set; }
}