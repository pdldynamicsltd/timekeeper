using CadentManagement.Authorization.Users.Dto;

namespace CadentManagement.Maui.Models.User;

public class UserListModel : UserListDto
{
    public string Photo { get; set; }

    public string FullName => Name + " " + Surname;
}