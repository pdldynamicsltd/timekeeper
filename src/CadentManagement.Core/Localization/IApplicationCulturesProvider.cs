using System.Globalization;

namespace CadentManagement.Localization;

public interface IApplicationCulturesProvider
{
    CultureInfo[] GetAllCultures();
}

