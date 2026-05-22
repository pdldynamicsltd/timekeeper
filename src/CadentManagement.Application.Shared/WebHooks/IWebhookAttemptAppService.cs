using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using CadentManagement.WebHooks.Dto;

namespace CadentManagement.WebHooks;

public interface IWebhookAttemptAppService
{
    Task<PagedResultDto<GetAllSendAttemptsOutput>> GetAllSendAttempts(GetAllSendAttemptsInput input);

    Task<ListResultDto<GetAllSendAttemptsOfWebhookEventOutput>> GetAllSendAttemptsOfWebhookEvent(GetAllSendAttemptsOfWebhookEventInput input);
}

