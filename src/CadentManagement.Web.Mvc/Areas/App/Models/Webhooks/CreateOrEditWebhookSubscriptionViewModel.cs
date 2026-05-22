using Abp.Application.Services.Dto;
using Abp.Webhooks;
using CadentManagement.WebHooks.Dto;

namespace CadentManagement.Web.Areas.App.Models.Webhooks;

public class CreateOrEditWebhookSubscriptionViewModel
{
    public WebhookSubscription WebhookSubscription { get; set; }

    public ListResultDto<GetAllAvailableWebhooksOutput> AvailableWebhookEvents { get; set; }
}

