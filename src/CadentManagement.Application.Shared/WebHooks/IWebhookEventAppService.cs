using System.Threading.Tasks;
using Abp.Webhooks;

namespace CadentManagement.WebHooks;

public interface IWebhookEventAppService
{
    Task<WebhookEvent> Get(string id);
}

