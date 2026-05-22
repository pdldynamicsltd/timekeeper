using System.ComponentModel.DataAnnotations;

namespace CadentManagement.Authorization.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}

