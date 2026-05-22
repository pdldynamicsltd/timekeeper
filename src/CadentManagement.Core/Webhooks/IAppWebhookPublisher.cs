using System.Threading.Tasks;
using CadentManagement.Authorization.Users;

namespace CadentManagement.WebHooks;

public interface IAppWebhookPublisher
{
    Task PublishTestWebhook();
}

