using System.ComponentModel.DataAnnotations;

namespace CadentManagement.Maui.Models.Login;

public class EmailActivationModel
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
}