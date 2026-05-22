using Abp.Dependency;
using CadentManagement.MultiTenancy.Payments;
using CadentManagement.Url;

namespace CadentManagement.Web.Url;

public class PaymentUrlGenerator : IPaymentUrlGenerator, ITransientDependency
{
    private readonly IWebUrlService _webUrlService;

    public PaymentUrlGenerator(IWebUrlService webUrlService)
    {
        _webUrlService = webUrlService;
    }

    public string CreatePaymentRequestUrl(SubscriptionPayment subscriptionPayment)
    {
        var webSiteRootAddress = _webUrlService.GetSiteRootAddress();

        return webSiteRootAddress +
               "account/gateway-selection?paymentId=" +
               subscriptionPayment.Id;
    }
}

