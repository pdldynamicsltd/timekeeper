using System.Threading.Tasks;
using CadentManagement.UiCustomization;

namespace CadentManagement.Test.Base.UiCustomization;

public class NullUiThemeCustomizerFactory : IUiThemeCustomizerFactory
{
    public Task<IUiCustomizer> GetCurrentUiCustomizer()
    {
        return Task.FromResult(new NullThemeUiCustomizer() as IUiCustomizer);
    }

    public IUiCustomizer GetUiCustomizer(string theme)
    {
        return new NullThemeUiCustomizer();
    }
}
