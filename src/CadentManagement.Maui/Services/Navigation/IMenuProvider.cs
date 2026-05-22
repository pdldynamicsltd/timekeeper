using CadentManagement.Maui.Models.NavigationMenu;

namespace CadentManagement.Maui.Services.Navigation;

public interface IMenuProvider
{
    List<NavigationMenuItem> GetAuthorizedMenuItems(Dictionary<string, string> grantedPermissions);
}