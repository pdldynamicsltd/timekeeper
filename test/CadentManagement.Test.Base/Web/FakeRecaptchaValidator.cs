using System.Threading.Tasks;
using CadentManagement.Security.Recaptcha;

namespace CadentManagement.Test.Base.Web;

public class FakeRecaptchaValidator : IRecaptchaValidator
{
    public Task ValidateAsync(string captchaResponse)
    {
        return Task.CompletedTask;
    }
}
