using System.Threading.Tasks;

namespace CadentManagement.Security.Recaptcha;

public interface IRecaptchaValidator
{
    Task ValidateAsync(string captchaResponse);
}
